using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AdvancedDLSupport;
using FRC.NetworkTables;
using FRC.NetworkTables.Strings;

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

            const string resourceRoot = "FRC.NetworkTables.Core.DesktopLibraries.Libraries.";

            var nativeLoader = new LibraryLoader(activator);

            nativeLoader.AddLibraryLocation(OsType.Windows32,
                resourceRoot + "Windows.x86.ntcore.dll");
            nativeLoader.AddLibraryLocation(OsType.Windows64,
                resourceRoot + "Windows.amd64.ntcore.dll");
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

        public static NT_Inst GetDefaultInstance()
        {
            return m_ntcore.NT_GetDefaultInstance();
        }

        public static NT_Inst CreateInstance()
        {
            return m_ntcore.NT_CreateInstance();
        }

        public static void DestroyInstance(NT_Inst inst)
        {
            m_ntcore.NT_DestroyInstance(inst);
        }

        public static NT_Inst GetInstanceFromHandle(NT_Handle handle)
        {
            return m_ntcore.NT_GetInstanceFromHandle(handle);
        }

        public static unsafe NT_Entry GetEntry(NT_Inst inst, string name)
        {
            CachedNativeString ns = UTF8String.CreateCachedUTF8String(name);
            return m_ntcore.NT_GetEntry(inst, ns.Buffer, ns.Length);
        }

        public static unsafe int GetEntryCount(NT_Inst inst, string prefix, NtType types)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            UIntPtr count = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntries(inst, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            m_ntcore.NT_DisposeEntryArray(data, count);
            return len;
        }

        public static unsafe Span<NT_Entry> GetEntries(NT_Inst inst, string prefix, NtType types, Span<NT_Entry> store)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            UIntPtr count = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntries(inst, ns.Buffer, ns.Length, (uint)types, &count);
            int len = (int)count;
            Span<NT_Entry> entries = GetSpanOrBuffer(store, len);
            new Span<NT_Entry>(data, len).CopyTo(entries);
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

        public static unsafe string GetEntryName(NT_Entry entry)
        {
            UIntPtr len = UIntPtr.Zero;
            var data = m_ntcore.NT_GetEntryName(entry, &len);
            string ret = UTF8String.ReadUTF8String(data, len);
            m_ntcore.NT_FreeCharArray(data);
            return ret;
        }

        public static NtType GetEntryType(NT_Entry entry)
        {
            return m_ntcore.NT_GetEntryType(entry);
        }

        public static ulong GetEntryLastChange(NT_Entry entry)
        {
            return m_ntcore.NT_GetEntryLastChange(entry);
        }

        public static unsafe NT_ManagedValue GetEntryValue(NT_Entry entry)
        {
            NT_Value value = new NT_Value();
            m_ntcore.NT_GetEntryValue(entry, &value);
            var ret = new NT_ManagedValue(&value);
            m_ntcore.NT_DisposeValue(&value);
            return ret;
        }

        public static unsafe bool SetDefaultEntryValue(NT_Entry entry, in NT_ManagedValue value)
        {
            NT_Value v = new NT_Value();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetDefaultEntryValue(entry, &v);
            NT_ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        public static unsafe bool SetEntryValue(NT_Entry entry, in NT_ManagedValue value)
        {
            NT_Value v = new NT_Value();
            value.CreateNativeFromManaged(&v);
            var ret = m_ntcore.NT_SetEntryValue(entry, &v);
            NT_ManagedValue.DisposeCreatedNative(&v);
            return ret.Get();
        }

        public static unsafe void ForceSetEntryValue(NT_Entry entry, in NT_ManagedValue value)
        {
            NT_Value v = new NT_Value();
            value.CreateNativeFromManaged(&v);
            m_ntcore.NT_SetEntryTypeValue(entry, &v);
            NT_ManagedValue.DisposeCreatedNative(&v);
        }

        public static unsafe void SetEntryTypeValue(NT_Entry entry, in NT_ManagedValue value)
        {
            NT_Value v = new NT_Value();
            value.CreateNativeFromManaged(&v);
            m_ntcore.NT_SetEntryTypeValue(entry, &v);
            NT_ManagedValue.DisposeCreatedNative(&v);
        }

        public static void SetEntryFlags(NT_Entry entry, EntryFlags flags)
        {
            m_ntcore.NT_SetEntryFlags(entry, (uint)flags);
        }

        public static EntryFlags GetEntryFlags(NT_Entry entry)
        {
            return (EntryFlags)m_ntcore.NT_GetEntryFlags(entry);
        }

        public static void DeleteEntry(NT_Entry entry)
        {
            m_ntcore.NT_DeleteEntry(entry);
        }

        public static void DeleteAllEntries(NT_Inst inst)
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

        public static unsafe EntryInfo? GetEntryInfoHandle(NetworkTableInstance inst, NT_Entry entry)
        {
            NT_EntryInfo info = new NT_EntryInfo();
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

        public static NT_EntryListenerPoller CreateEntryListenerPoller(NT_Inst inst)
        {
            return m_ntcore.NT_CreateEntryListenerPoller(inst);
        }

        public static unsafe Span<EntryNotification> PollEntryListener(NetworkTableInstance inst, NT_EntryListenerPoller poller, Span<EntryNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_EntryNotification* notifications = m_ntcore.NT_PollEntryListener(poller, &length);
            int len = (int)length;
            Span<EntryNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new EntryNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeEntryNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe Span<EntryNotification> PollEntryListener(NetworkTableInstance inst, NT_EntryListenerPoller poller, double timeout, out bool timedOut, Span<EntryNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_Bool timedOutNt = false;
            NT_EntryNotification* notifications = m_ntcore.NT_PollEntryListenerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<EntryNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new EntryNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeEntryNotificationArray(notifications, length);
            if (len == 0 && !timedOut) return null;
            return entries;
        }

        public static unsafe NT_EntryListener AddPolledEntryListener(NT_EntryListenerPoller poller, string prefix, NotifyFlags flags)
        {
            var ns = UTF8String.CreateCachedUTF8String(prefix);
            return m_ntcore.NT_AddPolledEntryListener(poller, ns.Buffer, ns.Length, (uint)flags);
        }

        public static unsafe NT_EntryListener AddPolledEntryListener(NT_EntryListenerPoller poller, NetworkTableEntry entry, NotifyFlags flags)
        {
            return m_ntcore.NT_AddPolledEntryListenerSingle(poller, entry.Handle, (uint)flags);
        }

        public static unsafe void RemoveEntryListener(NT_EntryListener listener)
        {
            m_ntcore.NT_RemoveEntryListener(listener);
        }

        public static unsafe bool WaitForEntryListenerQueue(NT_Inst inst, double timeout)
        {
            return m_ntcore.NT_WaitForEntryListenerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollEntryListener(NT_EntryListenerPoller poller)
        {
            m_ntcore.NT_CancelPollEntryListener(poller);
        }

        public static void DestroyEntryListenerPoller(NT_EntryListenerPoller poller)
        {
            m_ntcore.NT_DestroyEntryListenerPoller(poller);
        }



        public static NT_ConnectionListenerPoller CreateConnectionListenerPoller(NT_Inst inst)
        {
            return m_ntcore.NT_CreateConnectionListenerPoller(inst);
        }

        public static unsafe Span<ConnectionNotification> PollConnectionListener(NetworkTableInstance inst, NT_ConnectionListenerPoller poller, Span<ConnectionNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_ConnectionNotification* notifications = m_ntcore.NT_PollConnectionListener(poller, &length);
            int len = (int)length;
            Span<ConnectionNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new ConnectionNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeConnectionNotificationArray(notifications, length);
            return entries;
        }

        public static unsafe Span<ConnectionNotification> PollConnectionListener(NetworkTableInstance inst, NT_ConnectionListenerPoller poller, double timeout, out bool timedOut, Span<ConnectionNotification> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_Bool timedOutNt = false;
            NT_ConnectionNotification* notifications = m_ntcore.NT_PollConnectionListenerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<ConnectionNotification> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new ConnectionNotification(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeConnectionNotificationArray(notifications, length);
            if (len == 0 && !timedOut) return null;
            return entries;
        }

        public static unsafe NT_ConnectionListener AddPolledConnectionListener(NT_ConnectionListenerPoller poller, bool immediateNotify)
        {
            return m_ntcore.NT_AddPolledConnectionListener(poller, immediateNotify);
        }

        public static unsafe void RemoveConnectionListener(NT_ConnectionListener listener)
        {
            m_ntcore.NT_RemoveConnectionListener(listener);
        }

        public static unsafe bool WaitForConnectionListenerQueue(NT_Inst inst, double timeout)
        {
            return m_ntcore.NT_WaitForConnectionListenerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollConnectionListener(NT_ConnectionListenerPoller poller)
        {
            m_ntcore.NT_CancelPollConnectionListener(poller);
        }

        public static void DestroyConnectionListenerPoller(NT_ConnectionListenerPoller poller)
        {
            m_ntcore.NT_DestroyConnectionListenerPoller(poller);
        }




        public static NT_RpcCallPoller CreateRpcCallPoller(NT_Inst inst)
        {
            return m_ntcore.NT_CreateRpcCallPoller(inst);
        }

        public static unsafe Span<RpcAnswer> PollRpc(NetworkTableInstance inst, NT_RpcCallPoller poller, Span<RpcAnswer> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_RpcAnswer* notifications = m_ntcore.NT_PollRpc(poller, &length);
            int len = (int)length;
            Span<RpcAnswer> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new RpcAnswer(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeRpcAnswerArray(notifications, length);
            return entries;
        }

        public static unsafe Span<RpcAnswer> PollRpc(NetworkTableInstance inst, NT_RpcCallPoller poller, double timeout, out bool timedOut, Span<RpcAnswer> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_Bool timedOutNt = false;
            NT_RpcAnswer* notifications = m_ntcore.NT_PollRpcTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<RpcAnswer> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new RpcAnswer(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeRpcAnswerArray(notifications, length);
            if (len == 0 && !timedOut) return null;
            return entries;
        }

        public static unsafe void CreatePolledRpc(NT_Entry entry, ReadOnlySpan<byte> def, NT_RpcCallPoller poller)
        {
            fixed (byte* p = def)
            {
                m_ntcore.NT_CreatePolledRpc(entry, p, (UIntPtr)def.Length, poller);
            }
        }

        public static unsafe bool WaitForRpcCallQueue(NT_Inst inst, double timeout)
        {
            return m_ntcore.NT_WaitForRpcCallQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollRpc(NT_RpcCallPoller poller)
        {
            m_ntcore.NT_CancelPollRpc(poller);
        }

        public static void DestroyRpcCallPoller(NT_RpcCallPoller poller)
        {
            m_ntcore.NT_DestroyRpcCallPoller(poller);
        }


        public static NT_LoggerPoller CreateLoggerPoller(NT_Inst inst)
        {
            return m_ntcore.NT_CreateLoggerPoller(inst);
        }

        public static unsafe Span<LogMessage> PollLogger(NetworkTableInstance inst, NT_LoggerPoller poller, Span<LogMessage> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_LogMessage* notifications = m_ntcore.NT_PollLogger(poller, &length);
            int len = (int)length;
            Span<LogMessage> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new LogMessage(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeLogMessageArray(notifications, length);
            return entries;
        }

        public static unsafe Span<LogMessage> PollLogger(NetworkTableInstance inst, NT_LoggerPoller poller, double timeout, out bool timedOut, Span<LogMessage> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_Bool timedOutNt = false;
            NT_LogMessage* notifications = m_ntcore.NT_PollLoggerTimeout(poller, &length, timeout, &timedOutNt);
            timedOut = timedOutNt.Get();
            int len = (int)length;
            Span<LogMessage> entries = GetSpanOrBuffer(store, len);
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new LogMessage(inst, &notifications[i]);
            }
            m_ntcore.NT_DisposeLogMessageArray(notifications, length);
            if (len == 0 && !timedOut) return null;
            return entries;
        }

        public static unsafe NT_Logger AddPolledLogger(NT_LoggerPoller poller, int minLevel, int maxLevel)
        {
            return m_ntcore.NT_AddPolledLogger(poller, (uint)minLevel, (uint)maxLevel);
        }

        public static unsafe void RemoveLogger(NT_Logger logger)
        {
            m_ntcore.NT_RemoveLogger(logger);
        }

        public static unsafe bool WaitForLoggerQueue(NT_Inst inst, double timeout)
        {
            return m_ntcore.NT_WaitForLoggerQueue(inst, timeout).Get();
        }

        public static unsafe void CancelPollLogger(NT_LoggerPoller poller)
        {
            m_ntcore.NT_CancelPollLogger(poller);
        }

        public static void DestroyLoggerPoller(NT_LoggerPoller poller)
        {
            m_ntcore.NT_DestroyLoggerPoller(poller);
        }


        public static unsafe void PostRpcResponse(NT_Entry entry, NT_RpcCall call, ReadOnlySpan<byte> result)
        {
            fixed (byte* p = result)
            {
                UIntPtr len = (UIntPtr)result.Length;
                m_ntcore.NT_PostRpcResponse(entry, call, p, len);
            }
        }

        public static unsafe Span<byte> GetRpcResult(NT_Entry entry, NT_RpcCall call, Span<byte> store)
        {
            UIntPtr length = UIntPtr.Zero;
            byte* res = m_ntcore.NT_GetRpcResult(entry, call, &length);
            int len = (int)length;
            Span<byte> retVal = GetSpanOrBuffer(store, len);
            new Span<byte>(res, len).CopyTo(retVal);
            m_ntcore.NT_FreeCharArray(res);
            return retVal;
        }

        public static unsafe Span<byte> GetRpcResult(NT_Entry entry, NT_RpcCall call, double timeout, Span<byte> store)
        {
            UIntPtr length = UIntPtr.Zero;
            NT_Bool timedOut = false;
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

        public static unsafe void CancelRpcResult(NT_Entry entry, NT_RpcCall call)
        {
            m_ntcore.NT_CancelRpcResult(entry, call);
        }

        public static unsafe NT_RpcCall CallRpc(NT_Entry entry, Span<byte> @params)
        {
            fixed (byte* b = @params )
            {
                return m_ntcore.NT_CallRpc(entry, b, (UIntPtr)@params.Length);
            }
        }

        public static unsafe void SetNetworkIdentity(NT_Inst inst, String name)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(name))
            {
                m_ntcore.NT_SetNetworkIdentity(inst, str.Buffer, str.Length);
            }
        }

        public static unsafe NetworkMode GetNetworkMode(NT_Inst inst)
        {
            return (NetworkMode)m_ntcore.NT_GetNetworkMode(inst);
        }

        public static unsafe void StartServer(NT_Inst inst, string persistFilename, string listenAddress, int port)
        {
            using (var pStr = UTF8String.CreateUTF8DisposableString(persistFilename))
            using (var lStr = UTF8String.CreateUTF8DisposableString(listenAddress))
            {
                m_ntcore.NT_StartServer(inst, pStr.Buffer, lStr.Buffer, (uint)port);
            }
        }

        public static unsafe void StopServer(NT_Inst inst)
        {
            m_ntcore.NT_StopServer(inst);
        }

        public static unsafe void StartClient(NT_Inst inst)
        {
            m_ntcore.NT_StartClientNone(inst);
        }

        public static unsafe void StartClient(NT_Inst inst, string serverName, int port)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(serverName))
            {
                m_ntcore.NT_StartClient(inst, str.Buffer, (uint)port);
            }
        }

        public static unsafe void StartClient(NT_Inst inst, ReadOnlySpan<ServerPortPair> servers)
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

        public static unsafe void StartClientTeam(NT_Inst inst, int team, int port)
        {
            m_ntcore.NT_StartClientTeam(inst, (uint)team, (uint)port);
        }

        public static unsafe void StopClient(NT_Inst inst)
        {
            m_ntcore.NT_StopClient(inst);
        }

        public static unsafe void SetServer(NT_Inst inst, string serverName, int port)
        {
            using (var str = UTF8String.CreateUTF8DisposableString(serverName))
            {
                m_ntcore.NT_SetServer(inst, str.Buffer, (uint)port);
            }
        }

        public static unsafe void SetServer(NT_Inst inst, ReadOnlySpan<ServerPortPair> servers)
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

        public static unsafe void SetServerTeam(NT_Inst inst, int team, int port)
        {
            m_ntcore.NT_SetServerTeam(inst, (uint)team, (uint)port);
        }

        public static unsafe void StartDSClient(NT_Inst inst, int port)
        {
            m_ntcore.NT_StartDSClient(inst, (uint)port);
        }

        public static void StopDSClient(NT_Inst inst)
        {
            m_ntcore.NT_StopDSClient(inst);
        }

        public static unsafe void SetUpdateRate(NT_Inst inst, double interval)
        {
            m_ntcore.NT_SetUpdateRate(inst, interval);
        }

        public static unsafe void Flush(NT_Inst inst)
        {
            m_ntcore.NT_Flush(inst);
        }

        public static unsafe Span<ConnectionInfo> GetConnections(NT_Inst inst, Span<ConnectionInfo> store)
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

        public static unsafe bool IsConnected(NT_Inst inst)
        {
            return m_ntcore.NT_IsConnected(inst).Get();
        }

        public static unsafe void SavePersistent(NT_Inst inst, string filename)
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

        public static unsafe List<string> LoadPersistent(NT_Inst inst, string filename)
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

        public static unsafe void SaveEntries(NT_Inst inst, string filename, string prefix)
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

        public static unsafe List<string> LoadEntries(NT_Inst inst, string filename, string prefix)
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
