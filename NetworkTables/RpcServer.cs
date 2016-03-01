using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables.Extensions;
using NetworkTables.Logging;
using static NetworkTables.Logging.Logger;

namespace NetworkTables
{
    internal class RpcServer : IDisposable
    {
        internal struct RpcPair
        {
            public uint First { get; }
            public uint Second { get; }

            public RpcPair(uint first, uint second)
            {
                First = first;
                Second = second;
            }
        }

        private readonly Dictionary<RpcPair, SendMsgFunc> m_responseMap = new Dictionary<RpcPair, SendMsgFunc>();

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

        private bool m_active = false;

        public bool Active => m_active;

        public void Dispose()
        {
            Logger.Instance.SetDefaultLogger();
            m_terminating = true;
            m_pollCond.Set();
            Stop();
        }

        public delegate void SendMsgFunc(Message msg);

        public void Start()
        {
            lock (m_mutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_thread = new Thread(ThreadMain);
            m_thread.Name = "Rpc Thread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        public void Stop()
        {
            m_active = false;
            if (m_thread != null)
            {
                m_callCond.Set();
                //Join our dispatch thread.
                bool shutdown = m_thread.Join(TimeSpan.FromSeconds(1));
                //If it fails to join, abort the thread
                if (!shutdown) m_thread.Abort();
            }
        }

        public void ProcessRpc(string name, Message msg, RpcCallback func, uint connId, SendMsgFunc sendResponse)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (func != null)
                    m_callQueue.Enqueue(new RpcCall(name, msg, func, connId, sendResponse));
                else
                    m_pollQueue.Enqueue(new RpcCall(name, msg, func, connId, sendResponse));
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
            if (func != null)
            {
                m_callCond.Set();
            }
            else
            {
                m_pollCond.Set();
            }
        }

        public bool PollRpc(bool blocking, ref RpcCallInfo callInfo)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                while (m_pollQueue.Count == 0)
                {
                    if (!blocking || m_terminating) return false;
                    m_pollCond.Wait(m_mutex, ref lockEntered);
                }
                var item = m_pollQueue.Peek();
                uint callUid = (item.ConnId << 16) | item.Msg.SeqNumUid;
                callInfo.RpcId = (int)item.Msg.Id;
                callInfo.CallUid = callUid;
                callInfo.Name = item.Name;
                callInfo.Params = item.Msg.Str;
                m_responseMap.Add(new RpcPair(item.Msg.Id, callUid), item.SendResponse);
                m_pollQueue.Dequeue();
                return true;
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void PostRpcResponse(long rpcId, long callId, params byte[] result)
        {
            SendMsgFunc func = null;
            var pair = new RpcPair((uint)rpcId, (uint)callId);
            if (!m_responseMap.TryGetValue(pair, out func))
            {
                Warning("posting PRC response to nonexistent call (or duplicate response)");
                return;
            }
            func(Message.RpcResponse((uint)rpcId, (uint)callId, result));
            m_responseMap.Remove(pair);
        }

        private RpcServer()
        {
            m_active = false;
            m_terminating = false;
        }

        private void ThreadMain()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                while (m_active)
                {
                    while (m_callQueue.Count == 0)
                    {
                        m_callCond.Wait(m_mutex, ref lockEntered);
                        if (!m_active) return;
                    }
                    while (m_callQueue.Count != 0)
                    {
                        if (!m_active) return;
                        var item = m_callQueue.Dequeue();
                        Debug4($"rpc calling {item.Name}");

                        if (string.IsNullOrEmpty(item.Name) || item.Msg == null | item.Func == null ||
                            item.SendResponse == null)
                            continue;
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        var result = item.Func(item.Name, item.Msg.Val.GetRpc());
                        item.SendResponse(Message.RpcResponse(item.Msg.Id, item.Msg.SeqNumUid, result));
                        Monitor.Enter(m_mutex, ref lockEntered);
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        private bool m_terminating = false;

        private struct RpcCall
        {
            public RpcCall(string name, Message msg, RpcCallback func, uint connId, SendMsgFunc sendResponse)
            {
                Name = name;
                Msg = msg;
                Func = func;
                ConnId = connId;
                SendResponse = sendResponse;
            }

            public string Name { get; }
            public Message Msg { get; }
            public RpcCallback Func { get; }
            public uint ConnId { get; }
            public SendMsgFunc SendResponse { get; }

        }

        private readonly Queue<RpcCall> m_callQueue = new Queue<RpcCall>();
        private readonly Queue<RpcCall> m_pollQueue = new Queue<RpcCall>();

        private Thread m_thread;


        private readonly object m_mutex = new object();
        private readonly AutoResetEvent m_callCond = new AutoResetEvent(false);
        private readonly AutoResetEvent m_pollCond = new AutoResetEvent(false);
    }
}
