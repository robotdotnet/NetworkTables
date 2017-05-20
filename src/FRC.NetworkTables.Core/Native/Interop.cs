using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using FRC.NativeLibraryUtilities;
using NetworkTables.Tables;
using FRC;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Global

namespace NetworkTables.Core.Native
{
    internal class ExcludeFromCodeCoverageAttribute : Attribute
    {

    }
#if !NETSTANDARD
    [SuppressUnmanagedCodeSecurity]
#endif
    [ExcludeFromCodeCoverage]
    internal class Interop
    {
        private static readonly bool s_libraryLoaded;
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        internal static NativeLibraryLoader NativeLoader { get; }
        private static readonly string s_libraryLocation;
        private static readonly bool s_useCommandLineFile;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        private static readonly bool s_runFinalizer;

        // private constructor. Only used for our unload finalizer
        private Interop() { }
        private void Ping() { } // Used to force compilation
        // static variable used only for interop purposes
        private static readonly Interop finalizeInterop = new Interop();
        ~Interop()
        {
            // If we did not successfully get constructed, we don't need to destruct
            if (!s_runFinalizer) return;
            //Sets logger to null so no logger gets called back.
            NT_SetLogger(null, 0);

            NT_StopDSClient();
            NT_StopClient();
            NT_StopServer();
            NT_StopRpcServer();
            NT_StopNotifier();

            NativeLoader.LibraryLoader.UnloadLibrary();

            try
            {
                //Don't delete file if we are using a specified file.
                if (!s_useCommandLineFile && File.Exists(s_libraryLocation))
                {
                    File.Delete(s_libraryLocation);
                }
            }
            catch
            {
                //Any errors just ignore.
            }
        }


        static Interop()
        {
            if (!s_libraryLoaded)
            {
                try
                {
                    finalizeInterop.Ping();
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
                                s_libraryLocation = file;
                                s_useCommandLineFile = true;
                            }
                        }
                    }
                    
                    const string resourceRoot = "FRC.NetworkTables.Core.DesktopLibraries.Libraries.";

                    NativeLoader = new NativeLibraryLoader();
                    NativeLoader.AddLibraryLocation(OsType.Windows32,
                        resourceRoot + "Windows.x86.ntcore.dll");
                    NativeLoader.AddLibraryLocation(OsType.Windows64,
                        resourceRoot + "Windows.amd64.ntcore.dll");
                    NativeLoader.AddLibraryLocation(OsType.Linux32,
                        resourceRoot + "Linux.x86.libntcore.so");
                    NativeLoader.AddLibraryLocation(OsType.Linux64,
                        resourceRoot + "Linux.amd64.libntcore.so");
                    NativeLoader.AddLibraryLocation(OsType.MacOs32,
                        resourceRoot + "Mac_OS_X.x86.libntcore.dylib");
                    NativeLoader.AddLibraryLocation(OsType.MacOs64,
                        resourceRoot + "Mac_OS_X.x86_64.libntcore.dylib");
                    NativeLoader.AddLibraryLocation(OsType.roboRIO, "libntcore.so");

                    if (s_useCommandLineFile)
                    {
                        NativeLoader.LoadNativeLibrary<Interop>(s_libraryLocation, true);
                    }
                    else
                    {
                        NativeLoader.LoadNativeLibraryFromReflectedAssembly("FRC.NetworkTables.Core.DesktopLibraries");
                        s_libraryLocation = NativeLoader.LibraryLocation;
                    }

                    NativeDelegateInitializer.SetupNativeDelegates<Interop>(NativeLoader);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Environment.Exit(1);
                }
                s_runFinalizer = true;
                s_libraryLoaded = true;
            }
        }

        //Callback Typedefs
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NT_EntryListenerCallback(
            uint uid, IntPtr data, IntPtr name, UIntPtr name_len, IntPtr value, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NT_ConnectionListenerCallback(
            uint uid, IntPtr data, int connected, ref NtConnectionInfo conn);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NT_LogFunc(uint level, IntPtr file, uint line, IntPtr msg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WarmFunction(UIntPtr line, IntPtr msg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr NT_RPCCallback(
            IntPtr data, IntPtr name, UIntPtr name_len, IntPtr param, UIntPtr params_len, out UIntPtr results_len, ref NtConnectionInfo conn);


        //Interup Functions
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetEntryFlagsDelegate(IntPtr name, UIntPtr name_len, uint flags);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_GetEntryFlagsDelegate(IntPtr name, UIntPtr name_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DeleteEntryDelegate(IntPtr name, UIntPtr name_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DeleteAllEntriesDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryInfoDelegate(byte[] prefix, UIntPtr prefix_len, uint types, ref UIntPtr count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FlushDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_AddEntryListenerDelegate(byte[] prefix, UIntPtr prefix_len, IntPtr data, NT_EntryListenerCallback callback, uint flags);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_RemoveEntryListenerDelegate(uint entry_listener_uid);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_AddConnectionListenerDelegate(IntPtr data, NT_ConnectionListenerCallback callback, int immediate_notify);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_RemoveConnectionListenerDelegate(uint conn_listener_uid);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetNetworkIdentityDelegate(byte[] name, UIntPtr name_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartServerDelegate(byte[] persist_filename, byte[] listen_address, uint port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StopServerDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartClientNoneDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartClientDelegate(byte[] server_name, uint port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartClientMultiDelegate(UIntPtr count, IntPtr[] server_names, uint[] port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StopClientDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetServerDelegate(byte[] server_name, uint port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetServerMultiDelegate(UIntPtr count, IntPtr[] server_names, uint[] ports);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartDSClientDelegate(uint port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StopDSClientDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_StopRpcServerDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_StopNotifierDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_NotifierDestroyedDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetUpdateRateDelegate(double interval);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetConnectionsDelegate(ref UIntPtr count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_SavePersistentDelegate(byte[] filename);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_LoadPersistentDelegate(byte[] filename, WarmFunction warn);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeValueDelegate(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_InitValueDelegate(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeStringDelegate(ref NtStringRead str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate NtType NT_GetTypeDelegate(IntPtr name, UIntPtr name_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeConnectionInfoArrayDelegate(IntPtr arr, UIntPtr count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeEntryInfoArrayDelegate(IntPtr arr, UIntPtr count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ulong NT_NowDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetLoggerDelegate(NT_LogFunc funct, uint min_level);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_AllocateCharArrayDelegate(UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeBooleanArrayDelegate(IntPtr arr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeDoubleArrayDelegate(IntPtr arr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeCharArrayDelegate(IntPtr arr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeStringArrayDelegate(IntPtr arr, UIntPtr arr_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate NtType NT_GetValueTypeDelegate(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_GetValueBooleanDelegate(IntPtr value, ref ulong last_change, ref int v_boolean);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_GetValueDoubleDelegate(IntPtr value, ref ulong last_change, ref double v_double);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetValueStringDelegate(IntPtr value, ref ulong last_change, ref UIntPtr string_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetValueRawDelegate(IntPtr value, ref ulong last_change, ref UIntPtr raw_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetValueBooleanArrayDelegate(IntPtr value, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetValueDoubleArrayDelegate(IntPtr value, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetValueStringArrayDelegate(IntPtr value, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_GetEntryBooleanDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref int v_boolean);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_GetEntryDoubleDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref double v_double);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryStringDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref UIntPtr string_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryRawDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref UIntPtr raw_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryBooleanArrayDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryDoubleArrayDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryStringArrayDelegate(IntPtr name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryBooleanDelegate(IntPtr name, UIntPtr name_len, int v_boolean, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryDoubleDelegate(IntPtr name, UIntPtr name_len, double v_double, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryStringDelegate(IntPtr name, UIntPtr name_len, byte[] v_string, UIntPtr string_len, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryRawDelegate(IntPtr name, UIntPtr name_len, byte[] raw, UIntPtr raw_len, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryBooleanArrayDelegate(IntPtr name, UIntPtr name_len, int[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryDoubleArrayDelegate(IntPtr name, UIntPtr name_len, double[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryStringArrayDelegate(IntPtr name, UIntPtr name_len, DisposableNativeString[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryBooleanDelegate(IntPtr name, UIntPtr name_len, int v_boolean);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryDoubleDelegate(IntPtr name, UIntPtr name_len, double v_double);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryStringDelegate(IntPtr name, UIntPtr name_len, byte[] v_string, UIntPtr string_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryRawDelegate(IntPtr name, UIntPtr name_len, byte[] raw, UIntPtr raw_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryBooleanArrayDelegate(IntPtr name, UIntPtr name_len, int[] arr, UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryDoubleArrayDelegate(IntPtr name, UIntPtr name_len, double[] arr, UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetDefaultEntryStringArrayDelegate(IntPtr name, UIntPtr name_len, DisposableNativeString[] arr, UIntPtr size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_CreateRpcDelegate(IntPtr name, UIntPtr name_len, byte[] def, UIntPtr def_len, IntPtr data, NT_RPCCallback callback);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_CreatePolledRpcDelegate(IntPtr name, UIntPtr name_len, byte[] def, UIntPtr def_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_PollRpcTimeoutDelegate(int blocking, double time_out, out NtRpcCallInfo call_info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_PollRpcDelegate(int blocking, out NtRpcCallInfo call_info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_PostRpcResponseDelegate(uint rpc_id, uint call_uid, byte[] result, UIntPtr result_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_CallRpcDelegate(IntPtr name, UIntPtr name_len, byte[] param, UIntPtr params_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetRpcResultDelegate(int blocking, uint call_uid, ref UIntPtr result_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetRpcResultTimeoutDelegate(int blocking, uint call_uid, double time_out, ref UIntPtr result_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_CancelBlockingRpcResultDelegate(uint call_uid);


        [NativeDelegate] internal static NT_SetEntryFlagsDelegate NT_SetEntryFlags;
        [NativeDelegate] internal static NT_GetEntryFlagsDelegate NT_GetEntryFlags;
        [NativeDelegate] internal static NT_DeleteEntryDelegate NT_DeleteEntry;
        [NativeDelegate] internal static NT_DeleteAllEntriesDelegate NT_DeleteAllEntries;
        [NativeDelegate] internal static NT_GetEntryInfoDelegate NT_GetEntryInfo;
        [NativeDelegate] internal static NT_FlushDelegate NT_Flush;
        [NativeDelegate] internal static NT_AddEntryListenerDelegate NT_AddEntryListener;
        [NativeDelegate] internal static NT_RemoveEntryListenerDelegate NT_RemoveEntryListener;
        [NativeDelegate] internal static NT_AddConnectionListenerDelegate NT_AddConnectionListener;
        [NativeDelegate] internal static NT_RemoveConnectionListenerDelegate NT_RemoveConnectionListener;
        [NativeDelegate] internal static NT_SetNetworkIdentityDelegate NT_SetNetworkIdentity;
        [NativeDelegate] internal static NT_StartServerDelegate NT_StartServer;
        [NativeDelegate] internal static NT_StopServerDelegate NT_StopServer;
        [NativeDelegate] internal static NT_StartClientNoneDelegate NT_StartClientNone;
        [NativeDelegate] internal static NT_StartClientDelegate NT_StartClient;
        [NativeDelegate] internal static NT_StartClientMultiDelegate NT_StartClientMulti;
        [NativeDelegate] internal static NT_StopClientDelegate NT_StopClient;
        [NativeDelegate]
        internal static NT_SetServerDelegate NT_SetServer;
        [NativeDelegate]
        internal static NT_SetServerMultiDelegate NT_SetServerMulti;
        [NativeDelegate]
        internal static NT_StartDSClientDelegate NT_StartDSClient;
        [NativeDelegate]
        internal static NT_StopDSClientDelegate NT_StopDSClient;
        [NativeDelegate] internal static NT_StopRpcServerDelegate NT_StopRpcServer;
        [NativeDelegate] internal static NT_StopNotifierDelegate NT_StopNotifier;
        [NativeDelegate] internal static NT_NotifierDestroyedDelegate NT_NotifierDestroyed;
        [NativeDelegate] internal static NT_SetUpdateRateDelegate NT_SetUpdateRate;
        [NativeDelegate] internal static NT_GetConnectionsDelegate NT_GetConnections;
        [NativeDelegate] internal static NT_SavePersistentDelegate NT_SavePersistent;
        [NativeDelegate] internal static NT_LoadPersistentDelegate NT_LoadPersistent;
        [NativeDelegate] internal static NT_DisposeValueDelegate NT_DisposeValue;
        [NativeDelegate] internal static NT_InitValueDelegate NT_InitValue;
        [NativeDelegate] internal static NT_DisposeStringDelegate NT_DisposeString;
        [NativeDelegate] internal static NT_GetTypeDelegate NT_GetType;
        [NativeDelegate] internal static NT_DisposeConnectionInfoArrayDelegate NT_DisposeConnectionInfoArray;
        [NativeDelegate] internal static NT_DisposeEntryInfoArrayDelegate NT_DisposeEntryInfoArray;
        [NativeDelegate] internal static NT_NowDelegate NT_Now;
        [NativeDelegate] internal static NT_SetLoggerDelegate NT_SetLogger;
        [NativeDelegate] internal static NT_AllocateCharArrayDelegate NT_AllocateCharArray;
        [NativeDelegate] internal static NT_FreeBooleanArrayDelegate NT_FreeBooleanArray;
        [NativeDelegate] internal static NT_FreeDoubleArrayDelegate NT_FreeDoubleArray;
        [NativeDelegate] internal static NT_FreeCharArrayDelegate NT_FreeCharArray;
        [NativeDelegate] internal static NT_FreeStringArrayDelegate NT_FreeStringArray;
        [NativeDelegate] internal static NT_GetValueTypeDelegate NT_GetValueType;
        [NativeDelegate] internal static NT_GetValueBooleanDelegate NT_GetValueBoolean;
        [NativeDelegate] internal static NT_GetValueDoubleDelegate NT_GetValueDouble;
        [NativeDelegate] internal static NT_GetValueStringDelegate NT_GetValueString;
        [NativeDelegate] internal static NT_GetValueRawDelegate NT_GetValueRaw;
        [NativeDelegate] internal static NT_GetValueBooleanArrayDelegate NT_GetValueBooleanArray;
        [NativeDelegate] internal static NT_GetValueDoubleArrayDelegate NT_GetValueDoubleArray;
        [NativeDelegate] internal static NT_GetValueStringArrayDelegate NT_GetValueStringArray;
        [NativeDelegate] internal static NT_GetEntryBooleanDelegate NT_GetEntryBoolean;
        [NativeDelegate] internal static NT_GetEntryDoubleDelegate NT_GetEntryDouble;
        [NativeDelegate] internal static NT_GetEntryStringDelegate NT_GetEntryString;
        [NativeDelegate] internal static NT_GetEntryRawDelegate NT_GetEntryRaw;
        [NativeDelegate] internal static NT_GetEntryBooleanArrayDelegate NT_GetEntryBooleanArray;
        [NativeDelegate] internal static NT_GetEntryDoubleArrayDelegate NT_GetEntryDoubleArray;
        [NativeDelegate] internal static NT_GetEntryStringArrayDelegate NT_GetEntryStringArray;
        [NativeDelegate] internal static NT_SetEntryBooleanDelegate NT_SetEntryBoolean;
        [NativeDelegate] internal static NT_SetEntryDoubleDelegate NT_SetEntryDouble;
        [NativeDelegate] internal static NT_SetEntryStringDelegate NT_SetEntryString;
        [NativeDelegate] internal static NT_SetEntryRawDelegate NT_SetEntryRaw;
        [NativeDelegate] internal static NT_SetEntryBooleanArrayDelegate NT_SetEntryBooleanArray;
        [NativeDelegate] internal static NT_SetEntryDoubleArrayDelegate NT_SetEntryDoubleArray;
        [NativeDelegate] internal static NT_SetEntryStringArrayDelegate NT_SetEntryStringArray;

        [NativeDelegate] internal static NT_SetDefaultEntryBooleanDelegate NT_SetDefaultEntryBoolean;
        [NativeDelegate] internal static NT_SetDefaultEntryDoubleDelegate NT_SetDefaultEntryDouble;
        [NativeDelegate] internal static NT_SetDefaultEntryStringDelegate NT_SetDefaultEntryString;
        [NativeDelegate] internal static NT_SetDefaultEntryRawDelegate NT_SetDefaultEntryRaw;
        [NativeDelegate] internal static NT_SetDefaultEntryBooleanArrayDelegate NT_SetDefaultEntryBooleanArray;
        [NativeDelegate] internal static NT_SetDefaultEntryDoubleArrayDelegate NT_SetDefaultEntryDoubleArray;
        [NativeDelegate] internal static NT_SetDefaultEntryStringArrayDelegate NT_SetDefaultEntryStringArray;

        [NativeDelegate] internal static NT_CreateRpcDelegate NT_CreateRpc;
        [NativeDelegate] internal static NT_CreatePolledRpcDelegate NT_CreatePolledRpc;
        [NativeDelegate] internal static NT_PollRpcDelegate NT_PollRpc;
        [NativeDelegate] internal static NT_PollRpcTimeoutDelegate NT_PollRpcTimeout;
        [NativeDelegate] internal static NT_PostRpcResponseDelegate NT_PostRpcResponse;
        [NativeDelegate] internal static NT_CallRpcDelegate NT_CallRpc;
        [NativeDelegate] internal static NT_GetRpcResultDelegate NT_GetRpcResult;
        [NativeDelegate] internal static NT_GetRpcResultTimeoutDelegate NT_GetRpcResultTimeout;
        [NativeDelegate] internal static NT_CancelBlockingRpcResultDelegate NT_CancelBlockingRpcResult;
    }
}
