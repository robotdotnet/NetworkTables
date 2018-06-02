using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AdvancedDLSupport;
using FRC.NetworkTables.Strings;

[assembly: InternalsVisibleTo("FRC.NetworkTables.Core.Test")]

namespace FRC.NetworkTables.Interop
{
    public static class NtCore
    {
        private static INtCore m_ntcore;

        static NtCore()
        {
            var activator = new NativeLibraryBuilder(ImplementationOptions.UseIndirectCalls);

            string[] commandArgs = Environment.GetCommandLineArgs();
            foreach (var commandArg in commandArgs)
            {
                //search for a line with the prefix "-ntcore:"
                if (commandArg.ToLower().Contains("-ntcore:"))
                {
                    //Split line to get the library.
                    int splitLoc = commandArg.IndexOf(':');
                    string file = commandArg.Substring(splitLoc + 1);

                    //If the file exists, just return it so dlopen can load it.
                    if (File.Exists(file))
                    {
                        var library = activator.ActivateInterface<INtCore>(file);
                        m_ntcore = library;
                        return;
                    }
                }
            }

            const string resourceRoot = "FRC.NetworkTables.Core.DesktopLibraries.libraries.";

            var nativeLoader = new LibraryLoader(activator);

            nativeLoader.AddLibraryLocation(OsType.Windows32,
                resourceRoot + "windows.x86.ntcore.dll");
            nativeLoader.AddLibraryLocation(OsType.Windows64,
                resourceRoot + "windows.x86_64.ntcore.dll");
            nativeLoader.AddLibraryLocation(OsType.Linux32,
                resourceRoot + "Linux.x86.libntcore.so");
            nativeLoader.AddLibraryLocation(OsType.Linux64,
                resourceRoot + "Linux.amd64.libntcore.so");
            nativeLoader.AddLibraryLocation(OsType.MacOs32,
                resourceRoot + "Mac_OS_X.x86.libntcore.dylib");
            nativeLoader.AddLibraryLocation(OsType.MacOs64,
                resourceRoot + "Mac_OS_X.x86_64.libntcore.dylib");
            nativeLoader.AddLibraryLocation(OsType.roboRIO, "ntcore");

            m_ntcore = nativeLoader.LoadNativeLibraryFromReflectedAssembly<INtCore>("FRC.NetworkTables.Core.DesktopLibraries");
        }


        private static Span<T> GetSpanOrBuffer<T>(Span<T> store, int length)
        {
            return store.Length >= length ? store.Slice(0, length) : new T[length];
        }

        public static ulong Now()
        {
            return m_ntcore.NT_Now();
        }

        public static Instance GetDefaultInstance()
        {
            return m_ntcore.NT_GetDefaultInstance();
        }

        public static Instance CreateInstance()
        {
            return m_ntcore.NT_CreateInstance();
        }

        public static void DestroyInstance(Instance inst)
        {
            m_ntcore.NT_DestroyInstance(inst);
        }

        public static Instance GetInstanceFromHandle(Handle handle)
        {
            return m_ntcore.NT_GetInstanceFromHandle(handle);
        }

        public static unsafe Entry GetEntry(Instance inst, string name)
        {
            CachedNativeString ns = UTF8String.CreateCachedUTF8String(name);
            return m_ntcore.NT_GetEntry(inst, ns.Buffer, ns.Length);
        }

        public static unsafe int GetEntryCount(Instance inst, string prefix, NtType types)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            UIntPtr count = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntries(inst, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            m_ntcore.NT_DisposeEntryArray(data, count);
            return len;
        }

        public static unsafe Span<Entry> GetEntries(Instance inst, string prefix, NtType types, Span<Entry> store)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            UIntPtr count = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntries(inst, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            Span<Entry> entries = GetSpanOrBuffer(store, len);
            new Span<Entry>(data, len).CopyTo(entries);
            m_ntcore.NT_DisposeEntryArray(data, count);
            return entries;
        }

        public static unsafe Span<NetworkTableEntry> GetEntriesManaged(NetworkTableInstance inst, string prefix, NtType types, Span<NetworkTableEntry> store)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            UIntPtr count = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntries(inst.Handle, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            Span<NetworkTableEntry> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new NetworkTableEntry(inst, data[i]);
            }
            m_ntcore.NT_DisposeEntryArray(data, count);
            return entries;
        }

        public static unsafe string GetEntryName(Entry entry)
        {
            UIntPtr len = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntryName(entry, &len);
            string ret = UTF8String.ReadUTF8String(data, len);
            m_ntcore.NT_FreeCharArray(data);
            return ret;
        }

        public static NtType GetEntryType(Entry entry)
        {
            return m_ntcore.NT_GetEntryType(entry);
        }

        public static ulong GetEntryLastChange(Entry entry)
        {
            return m_ntcore.NT_GetEntryLastChange(entry);
        }

        public static unsafe ManagedValue GetEntryValue(Entry entry)
        {
            NtValue value = new NtValue();
            m_ntcore.NT_GetEntryValue(entry, &value);
            var ret = new ManagedValue(&value);
            m_ntcore.NT_DisposeValue(&value);
            return ret;
        }

        internal static unsafe RefManagedValue GetEntryRefValue(Entry entry)
        {
            NtValue value = new NtValue();
            m_ntcore.NT_GetEntryValue(entry, &value);
            var ret = new RefManagedValue(&value);
            m_ntcore.NT_DisposeValue(&value);
            return ret;
        }

        public static unsafe bool SetDefaultEntryValue(Entry entry, in ManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetDefaultEntryValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        public static unsafe bool SetEntryValue(Entry entry, in ManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetEntryValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        public static unsafe void SetEntryTypeValue(Entry entry, in ManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            m_ntcore.NT_SetEntryTypeValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
        }

        internal static unsafe bool SetDefaultEntryValue(Entry entry, in RefManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetDefaultEntryValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        internal static unsafe bool SetEntryValue(Entry entry, in RefManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetEntryValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        internal static unsafe void SetEntryTypeValue(Entry entry, in RefManagedValue value)
        {
            NtValue v = new NtValue();
            value.CreateNativeFromManaged(&v);
            m_ntcore.NT_SetEntryTypeValue(entry, &v);
            ManagedValue.DisposeCreatedNative(&v);
        }

        public static void SetEntryFlags(Entry entry, EntryFlags flags)
        {
            m_ntcore.NT_SetEntryFlags(entry, (uint)flags);
        }

        public static EntryFlags GetEntryFlags(Entry entry)
        {
            return (EntryFlags)m_ntcore.NT_GetEntryFlags(entry);
        }

        public static void DeleteEntry(Entry entry)
        {
            m_ntcore.NT_DeleteEntry(entry);
        }

        public static void DeleteAllEntries(Instance inst)
        {
            m_ntcore.NT_DeleteAllEntries(inst);
        }

        public static unsafe Span<EntryInfo> GetEntryInfo(NetworkTableInstance inst, string prefix, NtType types, Span<EntryInfo> store)
        {
            UIntPtr count = UIntPtr.Zero;
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            var data = m_ntcore.NT_GetEntryInfo(inst.Handle, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            Span<EntryInfo> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new EntryInfo(inst, &data[i]);
            }
            m_ntcore.NT_DisposeEntryInfoArray(data, count);
            return entries;
        }

        public static unsafe EntryInfo? GetEntryInfoHandle(NetworkTableInstance inst, Entry entry)
        {
            NtEntryInfo info = new NtEntryInfo();
            var ret = m_ntcore.NT_GetEntryInfoHandle(entry, &info);
            if (!ret.Get())
            {
                m_ntcore.NT_DisposeEntryInfo(&info);
                return null;
            }
            EntryInfo infoM = new EntryInfo(inst, &info);
            m_ntcore.NT_DisposeEntryInfo(&info);
            return infoM;
        }

        public static EntryListenerPoller CreateEntryListenerPoller(Instance inst)
        {
            return m_ntcore.NT_CreateEntryListenerPoller(inst);
        }

        public static unsafe Span<EntryNotification> PollEntryListener(NetworkTableInstance inst, EntryListenerPoller poller, Span<EntryNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtEntryNotification* notifications = m_ntcore.NT_PollEntryListener(poller, &length);
            int len = (int)length;
            Span<EntryNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new EntryNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeEntryNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe Span<EntryNotification> PollEntryListenerTimeout(NetworkTableInstance inst, EntryListenerPoller poller, double timeout, out bool timedOut, Span<EntryNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtBool timedOutNt = false;
            NtEntryNotification* notifications = m_ntcore.NT_PollEntryListenerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<EntryNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new EntryNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeEntryNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe EntryListener AddPolledEntryListener(EntryListenerPoller poller, string prefix, NotifyFlags flags)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            return m_ntcore.NT_AddPolledEntryListener(poller, ns.Buffer, ns.Length, (uint)flags);
        }

        public static unsafe EntryListener AddPolledEntryListener(EntryListenerPoller poller, NetworkTableEntry entry, NotifyFlags flags)
        {
            return m_ntcore.NT_AddPolledEntryListenerSingle(poller, entry.Handle, (uint)flags);
        }

        public static unsafe void RemoveEntryListener(EntryListener listener)
        {
            m_ntcore.NT_RemoveEntryListener(listener);
        }

        public static unsafe bool WaitForEntryListenerQueue(Instance inst, double timeout)
        {
            return m_ntcore.NT_WaitForEntryListenerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollEntryListener(EntryListenerPoller poller)
        {
            m_ntcore.NT_CancelPollEntryListener(poller);
        }

        public static void DestroyEntryListenerPoller(EntryListenerPoller poller)
        {
            m_ntcore.NT_DestroyEntryListenerPoller(poller);
        }



        public static ConnectionListenerPoller CreateConnectionListenerPoller(Instance inst)
        {
            return m_ntcore.NT_CreateConnectionListenerPoller(inst);
        }

        public static unsafe Span<ConnectionNotification> PollConnectionListener(NetworkTableInstance inst, ConnectionListenerPoller poller, Span<ConnectionNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtConnectionNotification* notifications = m_ntcore.NT_PollConnectionListener(poller, &length);
            int len = (int)length;
            Span<ConnectionNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new ConnectionNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeConnectionNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe Span<ConnectionNotification> PollConnectionListenerTimeout(NetworkTableInstance inst, ConnectionListenerPoller poller, double timeout, out bool timedOut, Span<ConnectionNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtBool timedOutNt = false;
            NtConnectionNotification* notifications = m_ntcore.NT_PollConnectionListenerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<ConnectionNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new ConnectionNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeConnectionNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe ConnectionListener AddPolledConnectionListener(ConnectionListenerPoller poller, bool immediateNotify)
        {
            return m_ntcore.NT_AddPolledConnectionListener(poller, immediateNotify);
        }

        public static unsafe void RemoveConnectionListener(ConnectionListener listener)
        {
            m_ntcore.NT_RemoveConnectionListener(listener);
        }

        public static unsafe bool WaitForConnectionListenerQueue(Instance inst, double timeout)
        {
            return m_ntcore.NT_WaitForConnectionListenerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollConnectionListener(ConnectionListenerPoller poller)
        {
            m_ntcore.NT_CancelPollConnectionListener(poller);
        }

        public static void DestroyConnectionListenerPoller(ConnectionListenerPoller poller)
        {
            m_ntcore.NT_DestroyConnectionListenerPoller(poller);
        }




        public static RpcCallPoller CreateRpcCallPoller(Instance inst)
        {
            return m_ntcore.NT_CreateRpcCallPoller(inst);
        }

        public static unsafe Span<RpcAnswer> PollRpc(NetworkTableInstance inst, RpcCallPoller poller, Span<RpcAnswer> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtRpcAnswer* notifications = m_ntcore.NT_PollRpc(poller, &length);
            int len = (int)length;
            Span<RpcAnswer> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new RpcAnswer(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeRpcAnswerArray(notifications, length);
            return entries;
        }

        public static unsafe Span<RpcAnswer> PollRpcTimeout(NetworkTableInstance inst, RpcCallPoller poller, double timeout, out bool timedOut, Span<RpcAnswer> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtBool timedOutNt = false;
            NtRpcAnswer* notifications = m_ntcore.NT_PollRpcTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<RpcAnswer> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new RpcAnswer(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeRpcAnswerArray(notifications, length);
            return entries;
        }

        public static unsafe void CreatePolledRpc(Entry entry, ReadOnlySpan<byte> def, RpcCallPoller poller)
        {
            fixed (byte* p = def)
            {
                m_ntcore.NT_CreatePolledRpc(entry, p, (UIntPtr)def.Length, poller);
            }
        }

        public static unsafe bool WaitForRpcCallQueue(Instance inst, double timeout)
        {
            return m_ntcore.NT_WaitForRpcCallQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollRpc(RpcCallPoller poller)
        {
            m_ntcore.NT_CancelPollRpc(poller);
        }

        public static void DestroyRpcCallPoller(RpcCallPoller poller)
        {
            m_ntcore.NT_DestroyRpcCallPoller(poller);
        }


        public static LoggerPoller CreateLoggerPoller(Instance inst)
        {
            return m_ntcore.NT_CreateLoggerPoller(inst);
        }

        public static unsafe Span<LogMessage> PollLogger(NetworkTableInstance inst, LoggerPoller poller, Span<LogMessage> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtLogMessage* notifications = m_ntcore.NT_PollLogger(poller, &length);
            int len = (int)length;
            Span<LogMessage> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new LogMessage(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeLogMessageArray(notifications, length);
            return entries;
        }

        public static unsafe Span<LogMessage> PollLoggerTimeout(NetworkTableInstance inst, LoggerPoller poller, double timeout, out bool timedOut, Span<LogMessage> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtBool timedOutNt = false;
            NtLogMessage* notifications = m_ntcore.NT_PollLoggerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<LogMessage> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new LogMessage(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeLogMessageArray(notifications, length);
            return entries;
        }

        public static unsafe Logger AddPolledLogger(LoggerPoller poller, int minLevel, int maxLevel)
        {
            return m_ntcore.NT_AddPolledLogger(poller, (uint)minLevel, (uint)maxLevel);
        }

        public static unsafe void RemoveLogger(Logger logger)
        {
            m_ntcore.NT_RemoveLogger(logger);
        }

        public static unsafe bool WaitForLoggerQueue(Instance inst, double timeout)
        {
            return m_ntcore.NT_WaitForLoggerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollLogger(LoggerPoller poller)
        {
            m_ntcore.NT_CancelPollLogger(poller);
        }

        public static void DestroyLoggerPoller(LoggerPoller poller)
        {
            m_ntcore.NT_DestroyLoggerPoller(poller);
        }


        public static unsafe void PostRpcResponse(Entry entry, RpcCall call, ReadOnlySpan<byte> result)
        {
            fixed (byte* p = result)
            {
                UIntPtr len = (UIntPtr)result.Length;
                m_ntcore.NT_PostRpcResponse(entry, call, p, len);
            }
        }

        public static unsafe Span<byte> GetRpcResult(Entry entry, RpcCall call, Span<byte> store)
        {
            UIntPtr length = UIntPtr.Zero;
            byte* res = m_ntcore.NT_GetRpcResult(entry, call, &length);
            int len = (int)length;
            Span<byte> retVal = GetSpanOrBuffer(store, len);
            new Span<byte>(res, len).CopyTo(retVal);
            m_ntcore.NT_FreeCharArray(res);
            return retVal;
        }

        public static unsafe Span<byte> GetRpcResult(Entry entry, RpcCall call, double timeout, Span<byte> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NtBool timedOut = false;
            byte* res = m_ntcore.NT_GetRpcResultTimeout(entry, call, &length, timeout, &timedOut);
            if (timedOut.Get())
            {
                return null;
            }
            int len = (int)length;
            Span<byte> retVal = GetSpanOrBuffer(store, len);
            new Span<byte>(res, len).CopyTo(retVal);
            m_ntcore.NT_FreeCharArray(res);
            return retVal;
        }

        public static unsafe void CancelRpcResult(Entry entry, RpcCall call)
        {
            m_ntcore.NT_CancelRpcResult(entry, call);
        }

        public static unsafe RpcCall CallRpc(Entry entry, Span<byte> @params)
        {
            fixed (byte* b = @params )
            {
                return m_ntcore.NT_CallRpc(entry, b, (UIntPtr)@params.Length);
            }
        }

        public static unsafe void SetNetworkIdentity(Instance inst, String name)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(name))
            {
                m_ntcore.NT_SetNetworkIdentity(inst, str.Buffer, str.Length);
            }
        }

        public static unsafe NetworkMode GetNetworkMode(Instance inst)
        {
            return (NetworkMode)m_ntcore.NT_GetNetworkMode(inst);
        }

        public static unsafe void StartServer(Instance inst, string persistFilename, string listenAddress, int port)
        {
            using (var pStr = UTF8String.CreateUTF8DisposableString(persistFilename))
            using (var lStr = UTF8String.CreateUTF8DisposableString(listenAddress))
            {
                m_ntcore.NT_StartServer(inst, pStr.Buffer, lStr.Buffer, (uint)port);
            }
        }

        public static unsafe void StopServer(Instance inst)
        {
            m_ntcore.NT_StopServer(inst);
        }

        public static unsafe void StartClient(Instance inst)
        {
            m_ntcore.NT_StartClientNone(inst);
        }

        public static unsafe void StartClient(Instance inst, string serverName, int port)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(serverName))
            {
                m_ntcore.NT_StartClient(inst, str.Buffer, (uint)port);
            }
        }

        public static unsafe void StartClient(Instance inst, ReadOnlySpan<ServerPortPair> servers)
        {
            var serverStrs = stackalloc byte*[servers.Length];
            var serverPorts = stackalloc uint[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                serverPorts[i] = (uint)servers[i].Port;
                string vStr = servers[i].Server;
                fixed (char* str = vStr)
                {
                    var encoding = Encoding.UTF8;
                    int bytes = encoding.GetByteCount(str, vStr.Length);
                    serverStrs[i] = (byte*)Marshal.AllocHGlobal((bytes + 1) * sizeof(byte));
                    encoding.GetBytes(str, vStr.Length, serverStrs[i], bytes);
                    serverStrs[i][bytes] = 0;
                }
            }
            m_ntcore.NT_StartClientMulti(inst, (UIntPtr)servers.Length, serverStrs, serverPorts);

            for (int i = 0; i < servers.Length; i++)
            {
                Marshal.FreeHGlobal((IntPtr)serverStrs[i]);
            }
        }

        public static unsafe void StartClientTeam(Instance inst, int team, int port)
        {
            m_ntcore.NT_StartClientTeam(inst, (uint)team, (uint)port);
        }

        public static unsafe void StopClient(Instance inst)
        {
            m_ntcore.NT_StopClient(inst);
        }

        public static unsafe void SetServer(Instance inst, string serverName, int port)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(serverName))
            {
                m_ntcore.NT_SetServer(inst, str.Buffer, (uint)port);
            }
        }

        public static unsafe void SetServer(Instance inst, ReadOnlySpan<ServerPortPair> servers)
        {
            var serverStrs = stackalloc byte*[servers.Length];
            var serverPorts = stackalloc uint[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                serverPorts[i] = (uint)servers[i].Port;
                string vStr = servers[i].Server;
                fixed (char* str = vStr)
                {
                    var encoding = Encoding.UTF8;
                    int bytes = encoding.GetByteCount(str, vStr.Length);
                    serverStrs[i] = (byte*)Marshal.AllocHGlobal((bytes + 1) * sizeof(byte));
                    encoding.GetBytes(str, vStr.Length, serverStrs[i], bytes);
                    serverStrs[i][bytes] = 0;
                }
            }
            m_ntcore.NT_SetServerMulti(inst, (UIntPtr)servers.Length, serverStrs, serverPorts);

            for (int i = 0; i < servers.Length; i++)
            {
                Marshal.FreeHGlobal((IntPtr)serverStrs[i]);
            }
        }

        public static unsafe void SetServerTeam(Instance inst, int team, int port)
        {
            m_ntcore.NT_SetServerTeam(inst, (uint)team, (uint)port);
        }

        public static unsafe void StartDSClient(Instance inst, int port)
        {
            m_ntcore.NT_StartDSClient(inst, (uint)port);
        }

        public static void StopDSClient(Instance inst)
        {
            m_ntcore.NT_StopDSClient(inst);
        }

        public static unsafe void SetUpdateRate(Instance inst, double interval)
        {
            m_ntcore.NT_SetUpdateRate(inst, interval);
        }

        public static unsafe void Flush(Instance inst)
        {
            m_ntcore.NT_Flush(inst);
        }

        public static unsafe Span<ConnectionInfo> GetConnections(Instance inst, Span<ConnectionInfo> store)
        {
            UIntPtr count = UIntPtr.Zero;
            var conns = m_ntcore.NT_GetConnections(inst, &count);
            int len = (int)count;
            Span<ConnectionInfo> retConns = GetSpanOrBuffer(store, len);
            for (int i = 0; i < retConns.Length; i++)
            {
                retConns[i] = new ConnectionInfo(&conns[i]);
            }
            m_ntcore.NT_DisposeConnectionInfoArray(conns, count);
            return retConns;
        }

        public static unsafe bool IsConnected(Instance inst)
        {
            return m_ntcore.NT_IsConnected(inst).Get();
        }

        public static unsafe void SavePersistent(Instance inst, string filename)
        {
            using (var f = UTF8String.CreateUTF8DisposableString(filename))
            {
                var error = m_ntcore.NT_SavePersistent(inst, f.Buffer);
                if (error != null)
                {
                    throw new PersistentException(UTF8String.ReadUTF8String(error));
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void WarnFunc(UIntPtr line, byte* messsage);

        public static unsafe List<string> LoadPersistent(Instance inst, string filename)
        {
            using (var f = UTF8String.CreateUTF8DisposableString(filename))
            {
                List<string> warns = new List<string>(4);
                var fp = Marshal.GetFunctionPointerForDelegate<WarnFunc>((UIntPtr line, byte* message) =>
                {
                    warns.Add($"{(int)line} : {UTF8String.ReadUTF8String(message)}");
                });
                var error = m_ntcore.NT_LoadPersistent(inst, f.Buffer, fp);
                if (error != null)
                {
                    throw new PersistentException(UTF8String.ReadUTF8String(error));
                }
                return warns;
            }
        }

        public static unsafe void SaveEntries(Instance inst, string filename, string prefix)
        {
            using (var f = UTF8String.CreateUTF8DisposableString(filename))
            using (var p = UTF8String.CreateUTF8DisposableString(prefix))
            {
                var error = m_ntcore.NT_SaveEntries(inst, f.Buffer, p.Buffer, p.Length);
                if (error != null)
                {
                    throw new PersistentException(UTF8String.ReadUTF8String(error));
                }
            }
        }

        public static unsafe List<string> LoadEntries(Instance inst, string filename, string prefix)
        {
            using (var f = UTF8String.CreateUTF8DisposableString(filename))
            using (var p = UTF8String.CreateUTF8DisposableString(prefix))
            {
                List<string> warns = new List<string>(4);
                var fp = Marshal.GetFunctionPointerForDelegate<WarnFunc>((UIntPtr line, byte* message) =>
                {
                    warns.Add($"{(int)line} : {UTF8String.ReadUTF8String(message)}");
                });
                var error = m_ntcore.NT_LoadEntries(inst, f.Buffer, p.Buffer, p.Length, fp);
                if (error != null)
                {
                    throw new PersistentException(UTF8String.ReadUTF8String(error));
                }
                return warns;
            }
        }


    }
}
