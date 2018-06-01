using FRC.NetworkTables.Interop;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FRC.NetworkTables
{
    public partial class NetworkTableInstance
    {
        private readonly ConcurrentDictionary<NT_EntryListener, InAction<EntryNotification>> m_entryListeners = new ConcurrentDictionary<NT_EntryListener, InAction<EntryNotification>>();
        private readonly Lazy<CancellationTokenSource> m_entryListenerToken;
        private Thread m_entryListenerThread;
        private NT_EntryListenerPoller m_entryListenerPoller;
        private readonly object m_entryListenerWaitQueueLock = new object();
        private bool m_entryListenerWaitQueue = false;

        private CancellationTokenSource CreateEntryListenerThread()
        {
            m_entryListenerPoller = NtCore.CreateEntryListenerPoller(Handle);
            CancellationTokenSource source = new CancellationTokenSource();
            var ret = new Thread(() =>
            {
                bool wasInterrupted = false;
                var token = source.Token;
                token.Register(() =>
                {
                    NtCore.CancelPollEntryListener(m_entryListenerPoller);
                });
                while (!token.IsCancellationRequested)
                {
                    var events = NtCore.PollEntryListener(this, m_entryListenerPoller);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (events == null)
                    {
                        lock (m_entryListenerWaitQueueLock)
                        {
                            if (m_entryListenerWaitQueue)
                            {
                                m_entryListenerWaitQueue = false;
                                Monitor.PulseAll(m_entryListenerWaitQueueLock);
                                continue;
                            }
                        }
                        wasInterrupted = true;
                        break;
                    }
                    foreach (var evnt in events)
                    {
                        if (m_entryListeners.TryGetValue(evnt.Listener, out var listener))
                        {
                            listener(evnt);
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                }
                lock (m_entryListenerWaitQueueLock)
                {
                    if (!wasInterrupted)
                    {
                        NtCore.DestroyEntryListenerPoller(m_entryListenerPoller);
                    }
                    m_entryListenerPoller = new NT_EntryListenerPoller();
                }
            })
            {
                Name = "NTEntryListener",
                IsBackground = true
            };
            ret.Start();
            m_entryListenerThread = ret;
            return source;
        }

        public NT_EntryListener AddEntryListener(string prefix, InAction<EntryNotification> listener, NotifyFlags flags)
        {
            var token = m_entryListenerToken.Value;
            var handle = NtCore.AddPolledEntryListener(m_entryListenerPoller, prefix, flags);
            m_entryListeners.GetOrAdd(handle, listener);
            return handle;
        }

        public NT_EntryListener AddEntryListener(in NetworkTableEntry entry, InAction<EntryNotification> listener, NotifyFlags flags)
        {
            var token = m_entryListenerToken.Value;
            var handle = NtCore.AddPolledEntryListener(m_entryListenerPoller, entry, flags);
            m_entryListeners.GetOrAdd(handle, listener);
            return handle;
        }

        public void RemoveEntryListener(NT_EntryListener listener)
        {
            NtCore.RemoveEntryListener(listener);
        }

        public bool WaitForEntryListenerQueue(double timeout)
        {
            if (!NtCore.WaitForEntryListenerQueue(Handle, timeout))
            {
                return false;
            }
            lock (m_entryListenerWaitQueueLock)
            {
                if (m_entryListenerPoller.Get() == 0) return true;
                m_entryListenerWaitQueue = true;
                NtCore.CancelPollEntryListener(m_entryListenerPoller);
                while (m_entryListenerWaitQueue)
                {
                    if (timeout < 0)
                    {
                        Monitor.Wait(m_entryListenerWaitQueueLock);
                    }
                    else
                    {
                        return Monitor.Wait(m_entryListenerWaitQueueLock, TimeSpan.FromSeconds(timeout));
                    }
                }
            }
            return true;
        }




        private readonly ConcurrentDictionary<NT_ConnectionListener, InAction<ConnectionNotification>> m_connectionListeners = new ConcurrentDictionary<NT_ConnectionListener, InAction<ConnectionNotification>>();
        private readonly Lazy<CancellationTokenSource> m_connectionListenerToken;
        private Thread m_connectionListenerThread;
        private NT_ConnectionListenerPoller m_connectionListenerPoller;
        private readonly object m_connectionListenerWaitQueueLock = new object();
        private bool m_connectionListenerWaitQueue = false;

        private CancellationTokenSource CreateConnectionListenerThread()
        {
            m_connectionListenerPoller = NtCore.CreateConnectionListenerPoller(Handle);
            CancellationTokenSource source = new CancellationTokenSource();
            var ret = new Thread(() =>
            {
                bool wasInterrupted = false;
                var token = source.Token;
                token.Register(() =>
                {
                    NtCore.CancelPollConnectionListener(m_connectionListenerPoller);
                });
                while (!token.IsCancellationRequested)
                {
                    var events = NtCore.PollConnectionListener(this, m_connectionListenerPoller);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (events == null)
                    {
                        lock (m_connectionListenerWaitQueueLock)
                        {
                            if (m_entryListenerWaitQueue)
                            {
                                m_entryListenerWaitQueue = false;
                                Monitor.PulseAll(m_connectionListenerWaitQueueLock);
                                continue;
                            }
                        }
                        wasInterrupted = true;
                        break;
                    }
                    foreach (var evnt in events)
                    {
                        if (m_connectionListeners.TryGetValue(evnt.Listener, out var listener))
                        {
                            listener(evnt);
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                }
                lock (m_connectionListenerWaitQueueLock)
                {
                    if (!wasInterrupted)
                    {
                        NtCore.DestroyConnectionListenerPoller(m_connectionListenerPoller);
                    }
                    m_connectionListenerPoller = new NT_ConnectionListenerPoller();
                }
            })
            {
                Name = "NTConnectionListener",
                IsBackground = true
            };
            ret.Start();
            m_connectionListenerThread = ret;
            return source;
        }

        public NT_ConnectionListener AddConnectionListener(InAction<ConnectionNotification> listener, bool immediateNotify)
        {
            var token = m_connectionListenerToken.Value;
            var handle = NtCore.AddPolledConnectionListener(m_connectionListenerPoller, immediateNotify);
            m_connectionListeners.GetOrAdd(handle, listener);
            return handle;
        }

        public void RemoveConnectionListener(NT_ConnectionListener listener)
        {
            m_connectionListeners.TryRemove(listener, out var _);
            NtCore.RemoveConnectionListener(listener);
        }

        public bool WaitForConnectionListenerQueue(double timeout)
        {
            if (!NtCore.WaitForConnectionListenerQueue(Handle, timeout))
            {
                return false;
            }
            lock (m_connectionListenerWaitQueueLock)
            {
                if (m_connectionListenerPoller.Get() == 0) return true;
                m_connectionListenerWaitQueue = true;
                NtCore.CancelPollConnectionListener(m_connectionListenerPoller);
                while (m_connectionListenerWaitQueue)
                {
                    if (timeout < 0)
                    {
                        Monitor.Wait(m_connectionListenerWaitQueueLock);
                    }
                    else
                    {
                        return Monitor.Wait(m_connectionListenerWaitQueueLock, TimeSpan.FromSeconds(timeout));
                    }
                }
            }
            return true;
        }

        private readonly ConcurrentDictionary<NT_Entry, InAction<RpcAnswer>> m_rpcCalls = new ConcurrentDictionary<NT_Entry, InAction<RpcAnswer>>();
        private readonly Lazy<CancellationTokenSource> m_rpcListenerToken;
        private Thread m_rpcListenerThread;
        private NT_RpcCallPoller m_rpcListenerPoller;
        private readonly object m_rpcListenerWaitQueueLock = new object();
        private bool m_rpcListenerWaitQueue = false;

        private CancellationTokenSource CreateRpcListenerThread()
        {
            m_rpcListenerPoller = NtCore.CreateRpcCallPoller(Handle);
            CancellationTokenSource source = new CancellationTokenSource();
            var ret = new Thread(() =>
            {
                bool wasInterrupted = false;
                var token = source.Token;
                token.Register(() =>
                {
                    NtCore.CancelPollRpc(m_rpcListenerPoller);
                });
                while (!token.IsCancellationRequested)
                {
                    var events = NtCore.PollRpc(this, m_rpcListenerPoller);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (events == null)
                    {
                        lock (m_rpcListenerWaitQueueLock)
                        {
                            if (m_entryListenerWaitQueue)
                            {
                                m_entryListenerWaitQueue = false;
                                Monitor.PulseAll(m_rpcListenerWaitQueueLock);
                                continue;
                            }
                        }
                        wasInterrupted = true;
                        break;
                    }
                    foreach (var evnt in events)
                    {
                        if (m_rpcCalls.TryGetValue(evnt.EntryHandle, out var listener))
                        {
                            listener(evnt);
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                }
                lock (m_rpcListenerWaitQueueLock)
                {
                    if (!wasInterrupted)
                    {
                        NtCore.DestroyRpcCallPoller(m_rpcListenerPoller);
                    }
                    m_rpcListenerPoller = new NT_RpcCallPoller();
                }
            })
            {
                Name = "NTRpcCall",
                IsBackground = true
            };
            ret.Start();
            m_rpcListenerThread = ret;
            return source;
        }

        public void CreateRpc(in NetworkTableEntry entry, InAction<RpcAnswer> callback)
        {
            var token = m_rpcListenerToken.Value;
            Span<byte> def = stackalloc byte[1]{ 0 };
            NtCore.CreatePolledRpc(entry.Handle, def, m_rpcListenerPoller);
            m_rpcCalls.GetOrAdd(entry.Handle, callback);
        }

        public bool WaitForRpcCallQueue(double timeout)
        {
            if (!NtCore.WaitForRpcCallQueue(Handle, timeout))
            {
                return false;
            }
            lock (m_rpcListenerWaitQueueLock)
            {
                if (m_rpcListenerPoller.Get() == 0) return true;
                m_rpcListenerWaitQueue = true;
                NtCore.CancelPollRpc(m_rpcListenerPoller);
                while (m_rpcListenerWaitQueue)
                {
                    if (timeout < 0)
                    {
                        Monitor.Wait(m_rpcListenerWaitQueueLock);
                    }
                    else
                    {
                        return Monitor.Wait(m_rpcListenerWaitQueueLock, TimeSpan.FromSeconds(timeout));
                    }
                }
            }
            return true;
        }







        private readonly ConcurrentDictionary<NT_Logger, InAction<LogMessage>> m_loggerListeners = new ConcurrentDictionary<NT_Logger, InAction<LogMessage>>();
        private readonly Lazy<CancellationTokenSource> m_loggerListenerToken;
        private Thread m_loggerListenerThread;
        private NT_LoggerPoller m_loggerListenerPoller;
        private readonly object m_loggerListenerWaitQueueLock = new object();
        private bool m_loggerListenerWaitQueue = false;

        private CancellationTokenSource CreateLoggerThread()
        {
            m_loggerListenerPoller = NtCore.CreateLoggerPoller(Handle);
            CancellationTokenSource source = new CancellationTokenSource();
            var ret = new Thread(() =>
            {
                bool wasInterrupted = false;
                var token = source.Token;
                token.Register(() =>
                {
                    NtCore.CancelPollLogger(m_loggerListenerPoller);
                });
                while (!token.IsCancellationRequested)
                {
                    var events = NtCore.PollLogger(this, m_loggerListenerPoller);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (events == null)
                    {
                        lock (m_loggerListenerWaitQueueLock)
                        {
                            if (m_entryListenerWaitQueue)
                            {
                                m_entryListenerWaitQueue = false;
                                Monitor.PulseAll(m_loggerListenerWaitQueueLock);
                                continue;
                            }
                        }
                        wasInterrupted = true;
                        break;
                    }
                    foreach (var evnt in events)
                    {
                        if (m_loggerListeners.TryGetValue(evnt.Logger, out var listener))
                        {
                            listener(evnt);
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                }
                lock (m_loggerListenerWaitQueueLock)
                {
                    if (!wasInterrupted)
                    {
                        NtCore.DestroyLoggerPoller(m_loggerListenerPoller);
                    }
                    m_loggerListenerPoller = new NT_LoggerPoller();
                }
            })
            {
                Name = "NTLogger",
                IsBackground = true
            };
            ret.Start();
            m_loggerListenerThread = ret;
            return source;
        }

        public NT_Logger AddLogger(InAction<LogMessage> listener, int minLevel, int maxLevel)
        {
            var token = m_loggerListenerToken.Value;
            var handle = NtCore.AddPolledLogger(m_loggerListenerPoller, minLevel, maxLevel);
            m_loggerListeners.GetOrAdd(handle, listener);
            return handle;
        }

        public void RemoveLogger(NT_Logger listener)
        {
            m_loggerListeners.TryRemove(listener, out var _);
            NtCore.RemoveLogger(listener);
        }

        public bool WaitForLoggerQueue(double timeout)
        {
            if (!NtCore.WaitForLoggerQueue(Handle, timeout))
            {
                return false;
            }
            lock (m_loggerListenerWaitQueueLock)
            {
                if (m_loggerListenerPoller.Get() == 0) return true;
                m_loggerListenerWaitQueue = true;
                NtCore.CancelPollLogger(m_loggerListenerPoller);
                while (m_loggerListenerWaitQueue)
                {
                    if (timeout < 0)
                    {
                        Monitor.Wait(m_loggerListenerWaitQueueLock);
                    }
                    else
                    {
                        return Monitor.Wait(m_loggerListenerWaitQueueLock, TimeSpan.FromSeconds(timeout));
                    }
                }
            }
            return true;
        }
    }
}
