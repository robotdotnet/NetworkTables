using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Logging;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using static NetworkTables.Logging.Logger;

namespace NetworkTables
{
    internal class RpcServer : IDisposable
    {
        private readonly Dictionary<(uint First, uint Second), SendMsgFunc> m_responseMap = new Dictionary<(uint First, uint Second), SendMsgFunc>();

        private static RpcServer s_instance;

        /// <summary>
        /// Gets the local instance of Dispatcher
        /// </summary>
        public static RpcServer Instance
        {
            get
            {
                if (s_instance == null)
                {
                    RpcServer d = new RpcServer();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        public bool Active { get; private set; }

        public void Dispose()
        {
            Logger.Instance.SetDefaultLogger();
            m_terminating = true;
            //m_cancellationTokenSource.Cancel();
            using (m_lockObject.Lock())
            {
                m_callCond.NotifyAll();
                m_pollCond.NotifyAll();
            }

            //Join our dispatch thread.
            m_thread?.WaitAndUnwrapException();
        }

        public delegate void SendMsgFunc(Message msg);

        public void Start()
        {
            using (m_lockObject.Lock())
            {
                if (Active) return;
                Active = true;
            }

            //Start our task
            m_thread = Task.Factory.StartNew(ThreadMain, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            Active = false;
            if (m_thread != null)
            {
                using (m_lockObject.Lock())
                {
                    m_callCond.NotifyAll();
                    m_pollCond.NotifyAll();
                }
                //Join our dispatch thread.
                m_thread?.GetAwaiter().GetResult();
            }
        }

        public void ProcessRpc(string name, Message msg, RpcCallback func, uint connId, SendMsgFunc sendResponse, ref ConnectionInfo connInfo)
        {

            using (m_lockObject.Lock())
            {
                if (func != null)
                    m_callQueue.Enqueue(new RpcCall(name, msg, func, connId, sendResponse, connInfo));
                else
                // ReSharper disable once ExpressionIsAlwaysNull
                    m_pollQueue.Enqueue(new RpcCall(name, msg, func, connId, sendResponse, connInfo));
                if (func != null)
                {
                    m_callCond.NotifyAll();
                }
                else
                {
                    m_pollCond.NotifyAll();
                }
            }
            
        }

        public async Task<RpcCallInfo?> PollRpcAsync(CancellationToken token)
        {
            IDisposable monitor = null;
            try
            {
                monitor = await m_lockObject.LockAsync(token).ConfigureAwait(false);
                while (m_pollQueue.Count == 0)
                {
                    if (m_terminating) return null;
                    await m_pollCond.WaitAsync(token).ConfigureAwait(false);
                    if (token.IsCancellationRequested) return null;
                    if (m_terminating) return null;
                }
                var item = m_pollQueue.Dequeue();
                uint callUid;
                if (item.ConnId != 0xffff)
                    callUid = (item.ConnId << 16) | item.Msg.SeqNumUid;
                else
                    callUid = item.Msg.SeqNumUid;
                if (!item.Msg.Val.IsRpc()) return null;
                RpcCallInfo callInfo = new RpcCallInfo(item.Msg.Id, callUid, item.Name, item.Msg.Val.GetRpc());
                m_responseMap.Add((item.Msg.Id, callUid), item.SendResponse);
                return callInfo;
            }
            catch (OperationCanceledException)
            {
                // Operation canceled. Return null.
                return null;
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public bool PollRpc(bool blocking, out RpcCallInfo callInfo)
        {
            return PollRpc(blocking, Timeout.InfiniteTimeSpan, out callInfo);
        }

        public bool PollRpc(bool blocking, TimeSpan timeout, out RpcCallInfo callInfo)
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_lockObject.Lock();
                DateTime startTime = DateTime.UtcNow;
                while (m_pollQueue.Count == 0)
                {
                    if (!blocking || m_terminating)
                    {
                        callInfo = default(RpcCallInfo);
                        return false;
                    }
                    CancellationTokenSource source = new CancellationTokenSource();
                    var task = m_pollCond.WaitAsync(source.Token);
                    TimeSpan waitTimeout = timeout;
                    if (timeout != Timeout.InfiniteTimeSpan)
                    {
                        DateTime now = DateTime.UtcNow;
                        if (now < startTime + timeout)
                        {
                            // We still have time to wait.
                            waitTimeout = (startTime + timeout) - now;
                        }
                        else
                        {
                            // We're past the wait time. No need to wait anymore
                            waitTimeout = TimeSpan.Zero;
                        }
                    }
                    bool success = task.Wait(waitTimeout);
                    if (!success || m_terminating)
                    {
                        source.Cancel();
                        // Call wait again without timeout to wait for lock to be reaquired
                        // ReSharper disable once MethodSupportsCancellation
                        task.WaitWithoutException();
                        callInfo = default(RpcCallInfo);
                        return false;
                    }
                }
                var item = m_pollQueue.Dequeue();
                uint callUid;
                if (item.ConnId != 0xffff)
                    callUid = (item.ConnId << 16) | item.Msg.SeqNumUid;
                else
                    callUid = item.Msg.SeqNumUid;
                if (!item.Msg.Val.IsRpc())
                {
                    callInfo = default(RpcCallInfo);
                    return false;
                }
                callInfo = new RpcCallInfo(item.Msg.Id, callUid, item.Name, item.Msg.Val.GetRpc());
                m_responseMap.Add((item.Msg.Id, callUid), item.SendResponse);
                return true;
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public void PostRpcResponse(long rpcId, long callId, IList<byte> result)
        {
            var pair = ((uint)rpcId, (uint)callId);
            if (!m_responseMap.TryGetValue(pair, out SendMsgFunc func))
            {
                Warning(Logger.Instance, "posting PRC response to nonexistent call (or duplicate response)");
                return;
            }
            func(Message.RpcResponse((uint)rpcId, (uint)callId, result));
            m_responseMap.Remove(pair);
        }

        internal RpcServer()
        {
            Active = false;
            m_terminating = false;
            m_lockObject = new AsyncLock();
            m_callCond = new AsyncConditionVariable(m_lockObject);
            m_pollCond = new AsyncConditionVariable(m_lockObject);
        }

        private void ThreadMain()
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_lockObject.Lock();
                while (Active)
                {
                    while (m_callQueue.Count == 0)
                    {
                        m_callCond.Wait();
                        if (!Active) return;
                    }
                    while (m_callQueue.Count != 0)
                    {
                        if (!Active) return;
                        var item = m_callQueue.Dequeue();
                        Debug4(Logger.Instance, $"rpc calling {item.Name}");

                        if (string.IsNullOrEmpty(item.Name) || item.Msg == null | item.Func == null ||
                            item.SendResponse == null)
                            continue;
                        IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                        monitorToUnlock.Dispose();
                        var result = item.Func(item.Name, item.Msg.Val.GetRpc(), item.ConnInfo);
                        var response = Message.RpcResponse(item.Msg.Id, item.Msg.SeqNumUid, result);
                        item.SendResponse(response);
                        Interlocked.Exchange(ref monitor, m_lockObject.Lock());
                    }
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        private bool m_terminating;
        //private readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

        private struct RpcCall
        {
            public RpcCall(string name, Message msg, RpcCallback func, uint connId, SendMsgFunc sendResponse, ConnectionInfo connInfo)
            {
                Name = name;
                Msg = msg;
                Func = func;
                ConnId = connId;
                SendResponse = sendResponse;
                ConnInfo = connInfo;
            }

            public string Name { get; }
            public Message Msg { get; }
            public RpcCallback Func { get; }
            public uint ConnId { get; }
            public SendMsgFunc SendResponse { get; }
            public ConnectionInfo ConnInfo { get; }

        }

        private readonly Queue<RpcCall> m_callQueue = new Queue<RpcCall>();
        private readonly Queue<RpcCall> m_pollQueue = new Queue<RpcCall>();

        private Task m_thread;

        private readonly AsyncLock m_lockObject;
        private readonly AsyncConditionVariable m_callCond;
        private readonly AsyncConditionVariable m_pollCond;
    }
}
