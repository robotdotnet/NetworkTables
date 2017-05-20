using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Exceptions;
using System.Linq;
using FRC;
using static FRC.UTF8String;

namespace NetworkTables.Core.Native
{
    /// <summary>
    /// This class contains all the methods to interface with the native library.
    /// </summary>
    /// <remarks>
    /// This is the equivelent of the NetworkTablesJNI.cpp class in the official library. It is the 
    /// bridge between the native methods called via P/Invoke and the NetworkTables classes.
    /// Most of these are internal, however some of them can be used publicly, so the class is public,
    /// and specific methods are public.
    /// </remarks>
    internal static class CoreMethods
    {
        #region SetDefault

        internal static bool SetDefaultEntryValue(string name, Value value)
        {
            switch (value.Type)
            {
                case NtType.Boolean:
                    return SetDefaultEntryBoolean(name, value.GetBoolean());
                case NtType.Double:
                    return SetDefaultEntryDouble(name, value.GetDouble());
                case NtType.String:
                    return SetDefaultEntryString(name, value.GetString());
                case NtType.Raw:
                    return SetDefaultEntryRaw(name, value.GetRpc());
                case NtType.BooleanArray:
                    return SetDefaultEntryBooleanArray(name, value.GetBooleanArray());
                case NtType.DoubleArray:
                    return SetDefaultEntryDoubleArray(name, value.GetDoubleArray());
                case NtType.StringArray:
                    return SetDefaultEntryStringArray(name, value.GetStringArray());
                default:
                    return false;
            }
        }

        internal static bool SetDefaultEntryBoolean(string name, bool value)
        {
            var namePtr = CreateCachedUTF8String(name);
            int retVal = Interop.NT_SetDefaultEntryBoolean(namePtr.Buffer, namePtr.Length, value ? 1 : 0);
            return retVal != 0;
        }

        internal static bool SetDefaultEntryDouble(string name, double value)
        {
            var namePtr = CreateCachedUTF8String(name);
            int retVal = Interop.NT_SetDefaultEntryDouble(namePtr.Buffer, namePtr.Length, value);
            return retVal != 0;
        }

        internal static bool SetDefaultEntryString(string name, string value)
        {
            var namePtr = CreateCachedUTF8String(name);
            byte[] stringPtr = CreateUTF8String(value);
            int retVal = Interop.NT_SetDefaultEntryString(namePtr.Buffer, namePtr.Length, stringPtr, (UIntPtr)(stringPtr.Length - 1));
            return retVal != 0;
        }

        internal static bool SetDefaultEntryRaw(string name, IList<byte> value)
        {
            var namePtr = CreateCachedUTF8String(name);
            byte[] arr = value as byte[];
            int retVal = Interop.NT_SetDefaultEntryRaw(namePtr.Buffer, namePtr.Length, arr ?? value.ToArray(), (UIntPtr)value.Count);
            return retVal != 0;
        }

        internal static bool SetDefaultEntryBooleanArray(string name, IList<bool> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Value array cannot be null");
            var namePtr = CreateCachedUTF8String(name);

            int[] valueIntArr = new int[value.Count];
            for (int i = 0; i < value.Count; i++)
            {
                valueIntArr[i] = value[i] ? 1 : 0;
            }

            int retVal = Interop.NT_SetDefaultEntryBooleanArray(namePtr.Buffer, namePtr.Length, valueIntArr, (UIntPtr)valueIntArr.Length);

            return retVal != 0;
        }

        internal static bool SetDefaultEntryDoubleArray(string name, IList<double> value)
        {
            var namePtr = CreateCachedUTF8String(name);
            double[] arr = value as double[];
            int retVal = Interop.NT_SetDefaultEntryDoubleArray(namePtr.Buffer, namePtr.Length, arr ?? value.ToArray(), (UIntPtr)value.Count);

            return retVal != 0;
        }

        internal static bool SetDefaultEntryStringArray(string name, IList<string> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Value array cannot be null");
            var namePtr = CreateCachedUTF8String(name);

            DisposableNativeString[] ntStrings = new DisposableNativeString[value.Count];
            for (int i = 0; i < value.Count; i++)
            {
                ntStrings[i] = new DisposableNativeString(value[i]);
            }

            int retVal = Interop.NT_SetDefaultEntryStringArray(namePtr.Buffer, namePtr.Length, ntStrings, (UIntPtr)ntStrings.Length);

            foreach (var ntString in ntStrings)
            {
                ntString.Dispose();
            }

            return retVal != 0;
        }

        #endregion

        #region Setters

        internal static bool SetEntryValue(string name, Value value, bool force = false)
        {
            switch (value.Type)
            {
                case NtType.Boolean:
                    return SetEntryBoolean(name, value.GetBoolean(), force);
                case NtType.Double:
                    return SetEntryDouble(name, value.GetDouble(), force);
                case NtType.String:
                    return SetEntryString(name, value.GetString(), force);
                case NtType.Raw:
                    return SetEntryRaw(name, value.GetRpc(), force);
                case NtType.BooleanArray:
                    return SetEntryBooleanArray(name, value.GetBooleanArray(), force);
                case NtType.DoubleArray:
                    return SetEntryDoubleArray(name, value.GetDoubleArray(), force);
                case NtType.StringArray:
                    return SetEntryStringArray(name, value.GetStringArray(), force);
                default:
                    return false;
            }
        }

        internal static bool SetEntryBoolean(string name, bool value, bool force = false)
        {
            var namePtr = CreateCachedUTF8String(name);
            int retVal = Interop.NT_SetEntryBoolean(namePtr.Buffer, namePtr.Length, value ? 1 : 0, force ? 1 : 0);
            return retVal != 0;
        }

        internal static bool SetEntryDouble(string name, double value, bool force = false)
        {
            var namePtr = CreateCachedUTF8String(name);
            int retVal = Interop.NT_SetEntryDouble(namePtr.Buffer, namePtr.Length, value, force ? 1 : 0);
            return retVal != 0;
        }

        internal static bool SetEntryString(string name, string value, bool force = false)
        {
            var namePtr = CreateCachedUTF8String(name);
            byte[] stringPtr = CreateUTF8String(value);
            int retVal = Interop.NT_SetEntryString(namePtr.Buffer, namePtr.Length, stringPtr, (UIntPtr)(stringPtr.Length - 1), force ? 1 : 0);
            return retVal != 0;
        }

        internal static bool SetEntryRaw(string name, IList<byte> value, bool force = false)
        {
            var namePtr = CreateCachedUTF8String(name);
            byte[] arr = value as byte[];
            int retVal = Interop.NT_SetEntryRaw(namePtr.Buffer, namePtr.Length, arr ?? value.ToArray(), (UIntPtr)value.Count, force ? 1 : 0);
            return retVal != 0;
        }

        internal static bool SetEntryBooleanArray(string name, IList<bool> value, bool force = false)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Value array cannot be null");
            var namePtr = CreateCachedUTF8String(name);

            int[] valueIntArr = new int[value.Count];
            for (int i = 0; i < value.Count; i++)
            {
                valueIntArr[i] = value[i] ? 1 : 0;
            }

            int retVal = Interop.NT_SetEntryBooleanArray(namePtr.Buffer, namePtr.Length, valueIntArr, (UIntPtr)valueIntArr.Length, force ? 1 : 0);

            return retVal != 0;
        }

        internal static bool SetEntryDoubleArray(string name, IList<double> value, bool force = false)
        {
            var namePtr = CreateCachedUTF8String(name);
            double[] arr = value as double[];
            int retVal = Interop.NT_SetEntryDoubleArray(namePtr.Buffer, namePtr.Length, arr ?? value.ToArray(), (UIntPtr)value.Count, force ? 1 : 0);

            return retVal != 0;
        }

        internal static bool SetEntryStringArray(string name, IList<string> value, bool force = false)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Value array cannot be null");
            var namePtr = CreateCachedUTF8String(name);

            DisposableNativeString[] ntStrings = new DisposableNativeString[value.Count];
            for (int i = 0; i < value.Count; i++)
            {
                ntStrings[i] = new DisposableNativeString(value[i]);
            }

            int retVal = Interop.NT_SetEntryStringArray(namePtr.Buffer, namePtr.Length, ntStrings, (UIntPtr)ntStrings.Length, force ? 1 : 0);

            foreach (var ntString in ntStrings)
            {
                ntString.Dispose();
            }

            return retVal != 0;
        }

        #endregion

        #region DefaultGetters

        internal static bool GetEntryBoolean(string name, bool defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            int boolean = 0;
            ulong lc = 0;
            int status = Interop.NT_GetEntryBoolean(namePtr.Buffer, namePtr.Length, ref lc, ref boolean);
            if (status == 0)
            {
                return defaultValue;
            }
            return boolean != 0;
        }

        internal static double GetEntryDouble(string name, double defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            double retVal = 0;
            ulong lastChange = 0;
            int status = Interop.NT_GetEntryDouble(namePtr.Buffer, namePtr.Length, ref lastChange, ref retVal);
            if (status == 0)
            {
                return defaultValue;
            }
            return retVal;
        }

        internal static string GetEntryString(string name, string defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr stringSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr ret = Interop.NT_GetEntryString(namePtr.Buffer, namePtr.Length, ref lastChange, ref stringSize);
            if (ret == IntPtr.Zero)
            {
                return defaultValue;
            }
            else
            {
                string str = ReadUTF8String(ret, stringSize);
                Interop.NT_FreeCharArray(ret);
                return str;
            }
        }

        internal static byte[] GetEntryRaw(string name, byte[] defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr stringSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr ret = Interop.NT_GetEntryRaw(namePtr.Buffer, namePtr.Length, ref lastChange, ref stringSize);
            if (ret == IntPtr.Zero)
            {
                return defaultValue;
            }
            byte[] arr = GetRawDataFromPtr(ret, stringSize);
            Interop.NT_FreeCharArray(ret);
            return arr;
        }

        internal static double[] GetEntryDoubleArray(string name, double[] defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryDoubleArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                return defaultValue;
            }
            double[] arr = GetDoubleArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeDoubleArray(arrPtr);
            return arr;
        }

        internal static bool[] GetEntryBooleanArray(string name, bool[] defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryBooleanArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                return defaultValue;
            }
            bool[] arr = GetBooleanArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeBooleanArray(arrPtr);
            return arr;
        }

        internal static string[] GetEntryStringArray(string name, string[] defaultValue)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryStringArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                return defaultValue;
            }
            string[] arr = GetStringArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeStringArray(arrPtr, arrSize);
            return arr;
        }

        #endregion

        #region Getters

        internal static Value GetEntryValue(string name)
        {
            NtType type = GetType(name);
            try
            {
                switch (type)
                {
                    case NtType.Boolean:
                        return Value.MakeBoolean(GetEntryBoolean(name));
                    case NtType.Double:
                        return Value.MakeDouble(GetEntryDouble(name));
                    case NtType.String:
                        return Value.MakeString(GetEntryString(name));
                    case NtType.Raw:
                        return Value.MakeRaw(GetEntryRaw(name));
                    case NtType.BooleanArray:
                        return Value.MakeBooleanArray(GetEntryBooleanArray(name));
                    case NtType.DoubleArray:
                        return Value.MakeDoubleArray(GetEntryDoubleArray(name));
                    case NtType.StringArray:
                        return Value.MakeStringArray(GetEntryStringArray(name));
                    default:
                        return null;
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static void ThrowException(string name, IntPtr namePtr, UIntPtr size, NtType requestedType)
        {
            NtType typeInTable = Interop.NT_GetType(namePtr, size);
            if (typeInTable == NtType.Unassigned)
            {
                throw new TableKeyNotDefinedException(name);
            }
            else
            {
                throw new TableKeyDifferentTypeException(name, requestedType, typeInTable);
            }
        }

        internal static bool GetEntryBoolean(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            int boolean = 0;
            ulong lc = 0;
            int status = Interop.NT_GetEntryBoolean(namePtr.Buffer, namePtr.Length, ref lc, ref boolean);
            if (status == 0)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.Boolean);
            }
            return boolean != 0;
        }

        internal static double GetEntryDouble(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            double retVal = 0;
            ulong lastChange = 0;
            int status = Interop.NT_GetEntryDouble(namePtr.Buffer, namePtr.Length, ref lastChange, ref retVal);
            if (status == 0)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.Double);
            }
            return retVal;
        }

        internal static string GetEntryString(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr stringSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr ret = Interop.NT_GetEntryString(namePtr.Buffer, namePtr.Length, ref lastChange, ref stringSize);
            if (ret == IntPtr.Zero)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.String);
            }
            string str = ReadUTF8String(ret, stringSize);
            Interop.NT_FreeCharArray(ret);
            return str;
        }

        internal static byte[] GetEntryRaw(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr stringSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr ret = Interop.NT_GetEntryRaw(namePtr.Buffer, namePtr.Length, ref lastChange, ref stringSize);
            if (ret == IntPtr.Zero)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.Raw);
            }
            byte[] data = GetRawDataFromPtr(ret, stringSize);
            Interop.NT_FreeCharArray(ret);
            return data;
        }

        internal static double[] GetEntryDoubleArray(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryDoubleArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.DoubleArray);
            }
            double[] arr = GetDoubleArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeDoubleArray(arrPtr);
            return arr;
        }

        internal static bool[] GetEntryBooleanArray(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryBooleanArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.BooleanArray);
            }
            bool[] arr = GetBooleanArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeBooleanArray(arrPtr);
            return arr;
        }

        internal static string[] GetEntryStringArray(string name)
        {
            var namePtr = CreateCachedUTF8String(name);
            UIntPtr arrSize = UIntPtr.Zero;
            ulong lastChange = 0;
            IntPtr arrPtr = Interop.NT_GetEntryStringArray(namePtr.Buffer, namePtr.Length, ref lastChange, ref arrSize);
            if (arrPtr == IntPtr.Zero)
            {
                ThrowException(name, namePtr.Buffer, namePtr.Length, NtType.StringArray);
            }
            string[] arr = GetStringArrayFromPtr(arrPtr, arrSize);
            Interop.NT_FreeStringArray(arrPtr, arrSize);
            return arr;
        }

        #endregion

        #region EntryInfo

        internal static List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            byte[] str = CreateUTF8String(prefix);
            UIntPtr arrSize = UIntPtr.Zero;
            IntPtr arr = Interop.NT_GetEntryInfo(str, (UIntPtr)(str.Length - 1), (uint)types, ref arrSize);
#pragma warning disable CS0618
            int entryInfoSize = Marshal.SizeOf(typeof(NtEntryInfo));
#pragma warning restore CS0618
            int arraySize = (int)arrSize.ToUInt64();
            List<EntryInfo> entryArray = new List<EntryInfo>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(arr.ToInt64() + entryInfoSize * i);
#pragma warning disable CS0618
                NtEntryInfo info = (NtEntryInfo)Marshal.PtrToStructure(data, typeof(NtEntryInfo));
#pragma warning restore CS0618
                entryArray.Add(new EntryInfo(info.name.ToString(), info.type, (EntryFlags)info.flags, (long)info.last_change));
            }
            Interop.NT_DisposeEntryInfoArray(arr, arrSize);
            return entryArray;
        }

        #endregion

        #region ConnectionInfo

        internal static List<ConnectionInfo> GetConnections()
        {
            UIntPtr count = UIntPtr.Zero;
            IntPtr connections = Interop.NT_GetConnections(ref count);
#pragma warning disable CS0618
            int connectionInfoSize = Marshal.SizeOf(typeof(NtConnectionInfo));
#pragma warning restore CS0618
            int arraySize = (int)count.ToUInt64();

            List<ConnectionInfo> connectionsArray = new List<ConnectionInfo>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(connections.ToInt64() + connectionInfoSize * i);
#pragma warning disable CS0618
                var con = (NtConnectionInfo)Marshal.PtrToStructure(data, typeof(NtConnectionInfo));
#pragma warning restore CS0618
                connectionsArray.Add(new ConnectionInfo(con.RemoteId.ToString(), con.RemoteIp.ToString(), (int)con.RemotePort, (long)con.LastUpdate, (int)con.ProtocolVersion));
            }
            Interop.NT_DisposeConnectionInfoArray(connections, count);
            return connectionsArray;
        }

        #endregion

        #region EntryListeners

        private static readonly Dictionary<int, Interop.NT_EntryListenerCallback> s_entryCallbacks = new Dictionary<int, Interop.NT_EntryListenerCallback>();

        internal static int AddEntryListener(string prefix, EntryListenerCallback listener, NotifyFlags flags)
        {
            // ReSharper disable once InconsistentNaming
            Interop.NT_EntryListenerCallback modCallback = (uid, data, name, len, value, flags_) =>
            {
                NtType type = Interop.NT_GetValueType(value);
                Value obj;
                ulong lastChange = 0;
                UIntPtr size = UIntPtr.Zero;
                IntPtr ptr;
                switch (type)
                {
                    case NtType.Unassigned:
                        obj = null;
                        break;
                    case NtType.Boolean:
                        int boolean = 0;
                        Interop.NT_GetValueBoolean(value, ref lastChange, ref boolean);
                        obj = Value.MakeBoolean(boolean != 0);
                        break;
                    case NtType.Double:
                        double val = 0;
                        Interop.NT_GetValueDouble(value, ref lastChange, ref val);
                        obj = Value.MakeDouble(val);
                        break;
                    case NtType.String:
                        ptr = Interop.NT_GetValueString(value, ref lastChange, ref size);
                        obj = Value.MakeString(ReadUTF8String(ptr, size));
                        break;
                    case NtType.Raw:
                        ptr = Interop.NT_GetValueRaw(value, ref lastChange, ref size);
                        obj = Value.MakeRaw(GetRawDataFromPtr(ptr, size));
                        break;
                    case NtType.BooleanArray:
                        ptr = Interop.NT_GetValueBooleanArray(value, ref lastChange, ref size);
                        obj = Value.MakeBooleanArray(GetBooleanArrayFromPtr(ptr, size));
                        break;
                    case NtType.DoubleArray:
                        ptr = Interop.NT_GetValueDoubleArray(value, ref lastChange, ref size);
                        obj = Value.MakeDoubleArray(GetDoubleArrayFromPtr(ptr, size));
                        break;
                    case NtType.StringArray:
                        ptr = Interop.NT_GetValueStringArray(value, ref lastChange, ref size);
                        obj = Value.MakeStringArray(GetStringArrayFromPtr(ptr, size));
                        break;
                    case NtType.Rpc:
                        ptr = Interop.NT_GetValueRaw(value, ref lastChange, ref size);
                        obj = Value.MakeRpc(GetRawDataFromPtr(ptr, size));
                        break;
                    default:
                        obj = null;
                        break;
                }
                string key = ReadUTF8String(name, len);
                listener((int)uid, key, obj, (NotifyFlags)flags_);
            };
            byte[] prefixStr = CreateUTF8String(prefix);
            int retVal = (int)Interop.NT_AddEntryListener(prefixStr, (UIntPtr)(prefixStr.Length - 1), IntPtr.Zero, modCallback, (uint)flags);
            s_entryCallbacks.Add(retVal, modCallback);
            return retVal;
        }

        internal static void RemoveEntryListener(int uid)
        {
            Interop.NT_RemoveEntryListener((uint)uid);
            if (s_entryCallbacks.ContainsKey(uid))
            {
                s_entryCallbacks.Remove(uid);
            }
        }

        #endregion

        #region Connection Listeners

        private static readonly Dictionary<int, Interop.NT_ConnectionListenerCallback> s_connectionCallbacks = new Dictionary<int, Interop.NT_ConnectionListenerCallback>();

        internal static int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            Interop.NT_ConnectionListenerCallback modCallback = (uint uid, IntPtr data, int connected, ref NtConnectionInfo conn) =>
            {
                ConnectionInfo info = new ConnectionInfo(conn.RemoteId.ToString(), conn.RemoteIp.ToString(), (int)conn.RemotePort, (long)conn.LastUpdate, (int)conn.ProtocolVersion);
                callback((int)uid, connected != 0, info);
            };

            int retVal = (int)Interop.NT_AddConnectionListener(IntPtr.Zero, modCallback, immediateNotify ? 1 : 0);
            s_connectionCallbacks.Add(retVal, modCallback);
            return retVal;
        }

        internal static void RemoveConnectionListener(int uid)
        {
            Interop.NT_RemoveConnectionListener((uint)uid);
            if (s_connectionCallbacks.ContainsKey(uid))
            {
                s_connectionCallbacks.Remove(uid);
            }
        }

        #endregion

        #region Server and Client Methods

        internal static void SetNetworkIdentity(string name)
        {
            byte[] namePtr = CreateUTF8String(name);
            Interop.NT_SetNetworkIdentity(namePtr, (UIntPtr)(namePtr.Length - 1));
        }

        internal static void StartClient()
        {
            Interop.NT_StartClientNone();
        }

        internal static void StartClient(string serverName, uint port)
        {
            if (serverName == null)
            {
                throw new ArgumentNullException(nameof(serverName), "Server cannot be null");
            }
            byte[] serverNamePtr = CreateUTF8String(serverName);
            Interop.NT_StartClient(serverNamePtr, port);
        }

        internal static void StartClient(IList<NtIPAddress> servers)
        {
            uint[] uPorts = new uint[servers.Count];
            for (int i = 0; i < uPorts.Length; i++)
            {
                uPorts[i] = (uint)servers[i].Port;
            }
            IntPtr[] serv = new IntPtr[servers.Count];
            DisposableNativeString[] servHolder = new DisposableNativeString[servers.Count];
            UIntPtr len;
            for (int i = 0; i < serv.Length; i++)
            {
                servHolder[i] = new DisposableNativeString(servers[i].IpAddress);
                serv[i] = servHolder[i].Buffer;
            }
            len = (UIntPtr)servers.Count;
            Interop.NT_StartClientMulti(len, serv, uPorts);
            foreach (var s in servHolder)
            {
                s.Dispose();
            }
        }

        internal static void SetServer(string serverName, int port)
        {
            if (serverName == null)
            {
                throw new ArgumentNullException(nameof(serverName), "Server cannot be null");
            }
            byte[] serverNamePtr = CreateUTF8String(serverName);
            Interop.NT_SetServer(serverNamePtr, (uint)port);
        }

        internal static void SetServer(IList<NtIPAddress> servers)
        {
            uint[] uPorts = new uint[servers.Count];
            for (int i = 0; i < uPorts.Length; i++)
            {
                uPorts[i] = (uint)servers[i].Port;
            }
            IntPtr[] serv = new IntPtr[servers.Count];
            DisposableNativeString[] servHolder = new DisposableNativeString[servers.Count];
            UIntPtr len;
            for (int i = 0; i < serv.Length; i++)
            {
                servHolder[i] = new DisposableNativeString(servers[i].IpAddress);
                serv[i] = servHolder[i].Buffer;
            }
            len = (UIntPtr)servers.Count;
            Interop.NT_SetServerMulti(len, serv, uPorts);
            foreach (var s in servHolder)
            {
                s.Dispose();
            }
        }

        internal static void StartDSClient(int port)
        {
            Interop.NT_StartDSClient((uint)port);
        }

        internal static void StopDSClient()
        {
            Interop.NT_StopDSClient();
        }

        internal static void StartServer(string fileName, string listenAddress, int port)
        {
            var fileNamePtr = string.IsNullOrEmpty(fileName) ? new[] { (byte)0 } : CreateUTF8String(fileName);
            var listenAddressPtr = string.IsNullOrEmpty(fileName) ? new[] { (byte)0 } : CreateUTF8String(listenAddress);
            Interop.NT_StartServer(fileNamePtr, listenAddressPtr, (uint)port);
        }

        internal static void StopClient()
        {
            Interop.NT_StopClient();
        }

        internal static void StopServer()
        {
            Interop.NT_StopServer();
        }

        internal static void StopNotifier()
        {
            Interop.NT_StopNotifier();
            //Clear callback dictionaries
            s_entryCallbacks.Clear();
            s_connectionCallbacks.Clear();
        }

        internal static void StopRpcServer()
        {
            Interop.NT_StopRpcServer();
            // Clear callback dictionaries
            s_rpcCallbacks.Clear();
        }

        internal static void SetUpdateRate(double interval)
        {
            Interop.NT_SetUpdateRate(interval);
        }

        #endregion

        #region Persistent

        internal static string SavePersistent(string filename)
        {
            byte[] name = CreateUTF8String(filename);
            IntPtr err = Interop.NT_SavePersistent(name);
            if (err != IntPtr.Zero)
            {
                return ReadUTF8String(err);
            }
            else
            {
                return null;
            }
        }

        internal static string LoadPersistent(string filename, Action<int, string> warn)
        {
            byte[] name = CreateUTF8String(filename);
            IntPtr err = Interop.NT_LoadPersistent(name, (line, msg) => { warn((int)line, ReadUTF8String(msg)); });
            if (err != IntPtr.Zero)
            {
                return ReadUTF8String(err);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Flags

        internal static void SetEntryFlags(string name, EntryFlags flags)
        {
            var str = CreateCachedUTF8String(name);
            Interop.NT_SetEntryFlags(str.Buffer, str.Length, (uint)flags);
        }

        internal static EntryFlags GetEntryFlags(string name)
        {
            var str = CreateCachedUTF8String(name);
            uint flags = Interop.NT_GetEntryFlags(str.Buffer, str.Length);
            return (EntryFlags)flags;
        }

        #endregion

        #region Utility

        internal static void DeleteEntry(string name)
        {
            var str = CreateCachedUTF8String(name);
            Interop.NT_DeleteEntry(str.Buffer, str.Length);
        }

        internal static void DeleteAllEntries()
        {
            Interop.NT_DeleteAllEntries();
        }

        internal static void Flush()
        {
            Interop.NT_Flush();
        }

        internal static bool NotifierDestroyed()
        {
            return Interop.NT_NotifierDestroyed() != 0;
        }

        internal static NtType GetType(string name)
        {
            var str = CreateCachedUTF8String(name);
            NtType retVal = Interop.NT_GetType(str.Buffer, str.Length);
            return retVal;
        }

        internal static bool ContainsEntry(string name)
        {
            NtType val = GetType(name);
            return val != NtType.Unassigned;
        }

        internal static long Now()
        {
            return (long)Interop.NT_Now();
        }

        #endregion

        #region Logger

        private static Interop.NT_LogFunc s_nativeLog;

        /// <summary>
        /// Assigns a method to be called whenever a log statement occurs in the internal
        /// network table library.
        /// </summary>
        /// <param name="func">The log function to assign.</param>
        /// <param name="minLevel">The minimum level to log.</param>
        public static void SetLogger(LogFunc func, LogLevel minLevel)
        {
            s_nativeLog = (level, file, line, msg) =>
            {
                string message = ReadUTF8String(msg);
                string fileName = ReadUTF8String(file);

                func((LogLevel)level, fileName, (int)line, message);
            };

            Interop.NT_SetLogger(s_nativeLog, (uint)minLevel);
        }

        #endregion

        #region IntPtr to Array Conversions

        private static double[] GetDoubleArrayFromPtr(IntPtr ptr, UIntPtr size)
        {
            double[] arr = new double[size.ToUInt64()];
            Marshal.Copy(ptr, arr, 0, arr.Length);
            return arr;
        }

        internal static byte[] GetRawDataFromPtr(IntPtr ptr, UIntPtr size)
        {
            int len = (int)size.ToUInt64();
            byte[] data = new byte[len];
            Marshal.Copy(ptr, data, 0, len);
            return data;
        }

        private static bool[] GetBooleanArrayFromPtr(IntPtr ptr, UIntPtr size)
        {
            int iSize = (int)size.ToUInt64();

            bool[] bArr = new bool[iSize];
            for (int i = 0; i < iSize; i++)
            {
                bArr[i] = Marshal.ReadInt32(ptr, sizeof(int) * i) != 0;
            }
            return bArr;
        }

        private static string[] GetStringArrayFromPtr(IntPtr ptr, UIntPtr size)
        {
#pragma warning disable CS0618
            int ntStringSize = Marshal.SizeOf(typeof(NtStringRead));
#pragma warning restore CS0618
            int arraySize = (int)size.ToUInt64();
            string[] strArray = new string[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(ptr.ToInt64() + ntStringSize * i);
#pragma warning disable CS0618
                strArray[i] = Marshal.PtrToStructure(data, typeof(NtStringRead)).ToString();
#pragma warning restore CS0618
            }
            return strArray;
        }

        #endregion

        #region Rpc
        // Never queried because it is only used to save for the GC
        // ReSharper disable once CollectionNeverQueried.Global
        internal static readonly List<Interop.NT_RPCCallback> s_rpcCallbacks = new List<Interop.NT_RPCCallback>();

        internal static void CreateRpc(string name, IList<byte> def, RpcCallback callback)
        {
            Interop.NT_RPCCallback modCallback =
                (IntPtr data, IntPtr ptr, UIntPtr len, IntPtr intPtr, UIntPtr paramsLen, out UIntPtr resultsLen, ref NtConnectionInfo connInfo) =>
                {
                    string retName = ReadUTF8String(ptr, len);
                    byte[] param = GetRawDataFromPtr(intPtr, paramsLen);
                    IList<byte> cb = callback(retName, param, connInfo.ToManaged());
                    resultsLen = (UIntPtr)cb.Count;
                    IntPtr retPtr = Interop.NT_AllocateCharArray(resultsLen);
                    if (cb is byte[])
                    {
                        Marshal.Copy((byte[])cb, 0, retPtr, cb.Count);
                    }
                    else
                    {
                        for (int i = 0; i < cb.Count; i++)
                        {
                            // Must do a slow write if not a byte[]
                            Marshal.WriteByte(retPtr, i, cb[i]);
                        }
                    }

                    return retPtr;
                };
            var nameB = CreateCachedUTF8String(name);
            if (def is byte[] bArr)
            {
                Interop.NT_CreateRpc(nameB.Buffer, nameB.Length, bArr, (UIntPtr)def.Count, IntPtr.Zero, modCallback);
            }
            else
            {
                Interop.NT_CreateRpc(nameB.Buffer, nameB.Length, def.ToArray(), (UIntPtr)def.Count, IntPtr.Zero, modCallback);
            }

            s_rpcCallbacks.Add(modCallback);
        }

        internal static void CreatePolledRpc(string name, IList<byte> def)
        {
            var nameB = CreateCachedUTF8String(name);
            if (def is byte[] bArr)
            {
                Interop.NT_CreatePolledRpc(nameB.Buffer, nameB.Length, bArr, (UIntPtr)def.Count);
            }
            else
            {
                Interop.NT_CreatePolledRpc(nameB.Buffer, nameB.Length, def.ToArray(), (UIntPtr)def.Count);
            }
        }

        internal static bool PollRpc(bool blocking, TimeSpan timeout, out RpcCallInfo callInfo)
        {
            if (Interop.NT_PollRpcTimeout(blocking ? 1 : 0, timeout.TotalSeconds, out NtRpcCallInfo nativeInfo) == 0)
            {
                callInfo = new RpcCallInfo();
                return false;
            }
            callInfo = nativeInfo.ToManaged();
            return true;
        }

        internal static bool PollRpc(bool blocking, out RpcCallInfo callInfo)
        {
            int retVal = Interop.NT_PollRpc(blocking ? 1 : 0, out var nativeInfo);
            if (retVal == 0)
            {
                callInfo = new RpcCallInfo();
                return false;
            }
            callInfo = nativeInfo.ToManaged();
            return true;
        }

        internal static void PostRpcResponse(long rpcId, long callUid, IList<byte> result)
        {
            if (result is byte[] bArr)
            {
                Interop.NT_PostRpcResponse((uint)rpcId, (uint)callUid, bArr, (UIntPtr)result.Count);
            }
            else
            {
                Interop.NT_PostRpcResponse((uint)rpcId, (uint)callUid, result.ToArray(), (UIntPtr)result.Count);
            }
        }

        internal static long CallRpc(string name, IList<byte> param)
        {
            var nameB = CreateCachedUTF8String(name);
            if (param is byte[] bArr)
            {
                return Interop.NT_CallRpc(nameB.Buffer, nameB.Length, bArr, (UIntPtr)param.Count);
            }
            else
            {
                return Interop.NT_CallRpc(nameB.Buffer, nameB.Length, param.ToArray(), (UIntPtr)param.Count);
            }
        }

#if !NET40
        internal static async Task<byte[]> GetRpcResultAsync(long callUid, CancellationToken token)
        {
            token.Register(() =>
            {
                // must use a delegate to cancel the call.
                Interop.NT_CancelBlockingRpcResult((uint)callUid);
            });
            try
            {
                var result = await Task.Run(() =>
                {
                    bool success = GetRpcResult(true, callUid, out byte[] results);
                    if (success)
                    {
                        return results;
                    }
                    else
                    {
                        return null;
                    }
                }, token).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
#endif //NET40
        internal static bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out byte[] result)
        {
            UIntPtr size = UIntPtr.Zero;
            IntPtr retVal = Interop.NT_GetRpcResultTimeout(blocking ? 1 : 0, (uint)callUid, timeout.TotalSeconds, ref size);
            if (retVal == IntPtr.Zero)
            {
                result = null;
                return false;
            }
            result = GetRawDataFromPtr(retVal, size);
            return true;
        }

        internal static bool GetRpcResult(bool blocking, long callUid, out byte[] result)
        {
            UIntPtr size = UIntPtr.Zero;
            IntPtr retVal = Interop.NT_GetRpcResult(blocking ? 1 : 0, (uint)callUid, ref size);
            if (retVal == IntPtr.Zero)
            {
                result = null;
                return false;
            }
            result = GetRawDataFromPtr(retVal, size);
            return true;
        }

        #endregion

        #region IntPtrs To String Conversions

        #endregion
    }
}
