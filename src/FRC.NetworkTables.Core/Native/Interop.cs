using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
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
        private static readonly IntPtr s_library;
        private static readonly ILibraryLoader s_loader;
        private static readonly OsType s_osType;
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

            NT_StopClient();
            NT_StopServer();
            NT_StopRpcServer();
            NT_StopNotifier();

            s_loader.UnloadLibrary(s_library);

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
                    s_osType = LoaderUtilities.GetOsType();

                    if (!s_useCommandLineFile)
                    {

                        if (!LoaderUtilities.CheckOsValid(s_osType))
                            throw new InvalidOperationException("OS Not Supported");

                        string embeddedResourceLocation;
                        LoaderUtilities.GetLibraryName(s_osType, out embeddedResourceLocation, out s_libraryLocation);

                        bool successfullyExtracted = LoaderUtilities.ExtractLibrary(embeddedResourceLocation,
                            ref s_libraryLocation);

                        if (!successfullyExtracted)
                            throw new FileNotFoundException(
                                "Library file could not be found in the resorces. Please contact RobotDotNet for support for this issue");
                    }

                    if (s_osType == OsType.Armv7HardFloat)
                    {
                        //Make sure the proper libstdc++.so.6 gets extracted.
                        string resourceLocation = "NetworkTables.NativeLibraries.armv7.libstdc++.so";
                        string extractLoc = "libstdc++.so.6";
                        LoaderUtilities.ExtractLibrary(resourceLocation, ref extractLoc);
                    }

                    s_library = LoaderUtilities.LoadLibrary(s_libraryLocation, s_osType, out s_loader);

                    if (s_library == IntPtr.Zero)
                    {
                        if (s_osType == OsType.Armv7HardFloat)
                        {
                            //We are arm v7. We might need the special libstdc++.so.6;
                            Console.WriteLine("You are on an Arm V7 device. On most of these devices, a " 
                                + "special library needs to be loaded. This library has been extracted" +
                                " to the current directory. Please prepend your run command with\n" + 
                                "env LD_LIBRARY_PATH=.:LD_LIBRARY_PATH yourcommand\nand run again.");
                            throw new InvalidOperationException("Follow the instructions printed above to solve this error.");
                        }
                        else
                        {
                            throw new BadImageFormatException($"Library file {s_libraryLocation} could not be loaded successfully.");
                        }
                    }

                    InitializeDelegates(s_library, s_loader);
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

        private static void InitializeDelegates(IntPtr library, ILibraryLoader loader)
        {
#pragma warning disable CS0618
            NT_SetEntryFlags = (NT_SetEntryFlagsDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryFlags"), typeof(NT_SetEntryFlagsDelegate));
            NT_GetEntryFlags = (NT_GetEntryFlagsDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryFlags"), typeof(NT_GetEntryFlagsDelegate));
            NT_DeleteEntry = (NT_DeleteEntryDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DeleteEntry"), typeof(NT_DeleteEntryDelegate));
            NT_DeleteAllEntries = (NT_DeleteAllEntriesDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DeleteAllEntries"), typeof(NT_DeleteAllEntriesDelegate));
            NT_GetEntryInfo = (NT_GetEntryInfoDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryInfo"), typeof(NT_GetEntryInfoDelegate));
            NT_Flush = (NT_FlushDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_Flush"), typeof(NT_FlushDelegate));
            NT_AddEntryListener = (NT_AddEntryListenerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_AddEntryListener"), typeof(NT_AddEntryListenerDelegate));
            NT_RemoveEntryListener = (NT_RemoveEntryListenerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_RemoveEntryListener"), typeof(NT_RemoveEntryListenerDelegate));
            NT_AddConnectionListener = (NT_AddConnectionListenerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_AddConnectionListener"), typeof(NT_AddConnectionListenerDelegate));
            NT_RemoveConnectionListener = (NT_RemoveConnectionListenerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_RemoveConnectionListener"), typeof(NT_RemoveConnectionListenerDelegate));
            NT_SetNetworkIdentity = (NT_SetNetworkIdentityDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetNetworkIdentity"), typeof(NT_SetNetworkIdentityDelegate));
            NT_StartServer = (NT_StartServerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StartServer"), typeof(NT_StartServerDelegate));
            NT_StopServer = (NT_StopServerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StopServer"), typeof(NT_StopServerDelegate));
            NT_StartClient = (NT_StartClientDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StartClient"), typeof(NT_StartClientDelegate));
            NT_StartClientMulti = (NT_StartClientMultiDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StartClientMulti"), typeof(NT_StartClientMultiDelegate));
            NT_StopClient = (NT_StopClientDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StopClient"), typeof(NT_StopClientDelegate));
            NT_StopRpcServer = (NT_StopRpcServerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StopRpcServer"), typeof(NT_StopRpcServerDelegate));
            NT_StopNotifier = (NT_StopNotifierDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_StopNotifier"), typeof(NT_StopNotifierDelegate));
            NT_SetUpdateRate = (NT_SetUpdateRateDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetUpdateRate"), typeof(NT_SetUpdateRateDelegate));
            NT_GetConnections = (NT_GetConnectionsDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetConnections"), typeof(NT_GetConnectionsDelegate));
            NT_SavePersistent = (NT_SavePersistentDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SavePersistent"), typeof(NT_SavePersistentDelegate));
            NT_LoadPersistent = (NT_LoadPersistentDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_LoadPersistent"), typeof(NT_LoadPersistentDelegate));
            NT_DisposeValue = (NT_DisposeValueDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DisposeValue"), typeof(NT_DisposeValueDelegate));
            NT_InitValue = (NT_InitValueDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_InitValue"), typeof(NT_InitValueDelegate));
            NT_DisposeString = (NT_DisposeStringDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DisposeString"), typeof(NT_DisposeStringDelegate));
            NT_GetType = (NT_GetTypeDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetType"), typeof(NT_GetTypeDelegate));
            NT_DisposeConnectionInfoArray = (NT_DisposeConnectionInfoArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DisposeConnectionInfoArray"), typeof(NT_DisposeConnectionInfoArrayDelegate));
            NT_DisposeEntryInfoArray = (NT_DisposeEntryInfoArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_DisposeEntryInfoArray"), typeof(NT_DisposeEntryInfoArrayDelegate));
            NT_Now = (NT_NowDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_Now"), typeof(NT_NowDelegate));
            NT_SetLogger = (NT_SetLoggerDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetLogger"), typeof(NT_SetLoggerDelegate));
            NT_AllocateCharArray = (NT_AllocateCharArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_AllocateCharArray"), typeof(NT_AllocateCharArrayDelegate));
            NT_FreeBooleanArray = (NT_FreeBooleanArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_FreeBooleanArray"), typeof(NT_FreeBooleanArrayDelegate));
            NT_FreeDoubleArray = (NT_FreeDoubleArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_FreeDoubleArray"), typeof(NT_FreeDoubleArrayDelegate));
            NT_FreeCharArray = (NT_FreeCharArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_FreeCharArray"), typeof(NT_FreeCharArrayDelegate));
            NT_FreeStringArray = (NT_FreeStringArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_FreeStringArray"), typeof(NT_FreeStringArrayDelegate));
            NT_GetValueType = (NT_GetValueTypeDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueType"), typeof(NT_GetValueTypeDelegate));
            NT_GetValueBoolean = (NT_GetValueBooleanDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueBoolean"), typeof(NT_GetValueBooleanDelegate));
            NT_GetValueDouble = (NT_GetValueDoubleDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueDouble"), typeof(NT_GetValueDoubleDelegate));
            NT_GetValueString = (NT_GetValueStringDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueString"), typeof(NT_GetValueStringDelegate));
            NT_GetValueRaw = (NT_GetValueRawDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueRaw"), typeof(NT_GetValueRawDelegate));
            NT_GetValueBooleanArray = (NT_GetValueBooleanArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueBooleanArray"), typeof(NT_GetValueBooleanArrayDelegate));
            NT_GetValueDoubleArray = (NT_GetValueDoubleArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueDoubleArray"), typeof(NT_GetValueDoubleArrayDelegate));
            NT_GetValueStringArray = (NT_GetValueStringArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetValueStringArray"), typeof(NT_GetValueStringArrayDelegate));
            NT_GetEntryBoolean = (NT_GetEntryBooleanDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryBoolean"), typeof(NT_GetEntryBooleanDelegate));
            NT_GetEntryDouble = (NT_GetEntryDoubleDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryDouble"), typeof(NT_GetEntryDoubleDelegate));
            NT_GetEntryString = (NT_GetEntryStringDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryString"), typeof(NT_GetEntryStringDelegate));
            NT_GetEntryRaw = (NT_GetEntryRawDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryRaw"), typeof(NT_GetEntryRawDelegate));
            NT_GetEntryBooleanArray = (NT_GetEntryBooleanArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryBooleanArray"), typeof(NT_GetEntryBooleanArrayDelegate));
            NT_GetEntryDoubleArray = (NT_GetEntryDoubleArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryDoubleArray"), typeof(NT_GetEntryDoubleArrayDelegate));
            NT_GetEntryStringArray = (NT_GetEntryStringArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetEntryStringArray"), typeof(NT_GetEntryStringArrayDelegate));
            NT_SetEntryBoolean = (NT_SetEntryBooleanDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryBoolean"), typeof(NT_SetEntryBooleanDelegate));
            NT_SetEntryDouble = (NT_SetEntryDoubleDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryDouble"), typeof(NT_SetEntryDoubleDelegate));
            NT_SetEntryString = (NT_SetEntryStringDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryString"), typeof(NT_SetEntryStringDelegate));
            NT_SetEntryRaw = (NT_SetEntryRawDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryRaw"), typeof(NT_SetEntryRawDelegate));
            NT_SetEntryBooleanArray = (NT_SetEntryBooleanArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryBooleanArray"), typeof(NT_SetEntryBooleanArrayDelegate));
            NT_SetEntryDoubleArray = (NT_SetEntryDoubleArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryDoubleArray"), typeof(NT_SetEntryDoubleArrayDelegate));
            NT_SetEntryStringArray = (NT_SetEntryStringArrayDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_SetEntryStringArray"), typeof(NT_SetEntryStringArrayDelegate));
            NT_CreateRpc = (NT_CreateRpcDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_CreateRpc"), typeof(NT_CreateRpcDelegate));
            NT_CreatePolledRpc = (NT_CreatePolledRpcDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_CreatePolledRpc"), typeof(NT_CreatePolledRpcDelegate));
            NT_PollRpc = (NT_PollRpcDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_PollRpc"), typeof(NT_PollRpcDelegate));
            NT_PostRpcResponse = (NT_PostRpcResponseDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_PostRpcResponse"), typeof(NT_PostRpcResponseDelegate));
            NT_CallRpc = (NT_CallRpcDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_CallRpc"), typeof(NT_CallRpcDelegate));
            NT_GetRpcResult = (NT_GetRpcResultDelegate)Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, "NT_GetRpcResult"), typeof(NT_GetRpcResultDelegate));
#pragma warning restore CS0618
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
            IntPtr data, IntPtr name, UIntPtr name_len, IntPtr param, UIntPtr params_len, out UIntPtr results_len);


        //Interup Functions
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_SetEntryFlagsDelegate(byte[] name, UIntPtr name_len, uint flags);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_GetEntryFlagsDelegate(byte[] name, UIntPtr name_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DeleteEntryDelegate(byte[] name, UIntPtr name_len);
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
        internal delegate void NT_StartClientDelegate(byte[] server_name, uint port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StartClientMultiDelegate(UIntPtr count, IntPtr[] server_names, uint[] port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_StopClientDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_StopRpcServerDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_StopNotifierDelegate();
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
        internal delegate NtType NT_GetTypeDelegate(byte[] name, UIntPtr name_len);
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
        internal delegate int NT_GetEntryBooleanDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref int v_boolean);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_GetEntryDoubleDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref double v_double);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryStringDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref UIntPtr string_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryRawDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref UIntPtr raw_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryBooleanArrayDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryDoubleArrayDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryStringArrayDelegate(byte[] name, UIntPtr name_len, ref ulong last_change, ref UIntPtr size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryBooleanDelegate(byte[] name, UIntPtr name_len, int v_boolean, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryDoubleDelegate(byte[] name, UIntPtr name_len, double v_double, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryStringDelegate(byte[] name, UIntPtr name_len, byte[] v_string, UIntPtr string_len, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryRawDelegate(byte[] name, UIntPtr name_len, byte[] raw, UIntPtr raw_len, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryBooleanArrayDelegate(byte[] name, UIntPtr name_len, int[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryDoubleArrayDelegate(byte[] name, UIntPtr name_len, double[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_SetEntryStringArrayDelegate(byte[] name, UIntPtr name_len, NtStringWrite[] arr, UIntPtr size, int force);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_CreateRpcDelegate(byte[] name, UIntPtr name_len, byte[] def, UIntPtr def_len, IntPtr data, NT_RPCCallback callback);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_CreatePolledRpcDelegate(byte[] name, UIntPtr name_len, byte[] def, UIntPtr def_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int NT_PollRpcDelegate(int blocking, out NtRpcCallInfo call_info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_PostRpcResponseDelegate(uint rpc_id, uint call_uid, byte[] result, UIntPtr result_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint NT_CallRpcDelegate(byte[] name, UIntPtr name_len, byte[] param, UIntPtr params_len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetRpcResultDelegate(int blocking, uint call_uid, ref UIntPtr result_len);


        internal static NT_SetEntryFlagsDelegate NT_SetEntryFlags;
        internal static NT_GetEntryFlagsDelegate NT_GetEntryFlags;
        internal static NT_DeleteEntryDelegate NT_DeleteEntry;
        internal static NT_DeleteAllEntriesDelegate NT_DeleteAllEntries;
        internal static NT_GetEntryInfoDelegate NT_GetEntryInfo;
        internal static NT_FlushDelegate NT_Flush;
        internal static NT_AddEntryListenerDelegate NT_AddEntryListener;
        internal static NT_RemoveEntryListenerDelegate NT_RemoveEntryListener;
        internal static NT_AddConnectionListenerDelegate NT_AddConnectionListener;
        internal static NT_RemoveConnectionListenerDelegate NT_RemoveConnectionListener;
        internal static NT_SetNetworkIdentityDelegate NT_SetNetworkIdentity;
        internal static NT_StartServerDelegate NT_StartServer;
        internal static NT_StopServerDelegate NT_StopServer;
        internal static NT_StartClientDelegate NT_StartClient;
        internal static NT_StartClientMultiDelegate NT_StartClientMulti;
        internal static NT_StopClientDelegate NT_StopClient;
        internal static NT_StopRpcServerDelegate NT_StopRpcServer;
        internal static NT_StopNotifierDelegate NT_StopNotifier;
        internal static NT_SetUpdateRateDelegate NT_SetUpdateRate;
        internal static NT_GetConnectionsDelegate NT_GetConnections;
        internal static NT_SavePersistentDelegate NT_SavePersistent;
        internal static NT_LoadPersistentDelegate NT_LoadPersistent;
        internal static NT_DisposeValueDelegate NT_DisposeValue;
        internal static NT_InitValueDelegate NT_InitValue;
        internal static NT_DisposeStringDelegate NT_DisposeString;
        internal static NT_GetTypeDelegate NT_GetType;
        internal static NT_DisposeConnectionInfoArrayDelegate NT_DisposeConnectionInfoArray;
        internal static NT_DisposeEntryInfoArrayDelegate NT_DisposeEntryInfoArray;
        internal static NT_NowDelegate NT_Now;
        internal static NT_SetLoggerDelegate NT_SetLogger;
        internal static NT_AllocateCharArrayDelegate NT_AllocateCharArray;
        internal static NT_FreeBooleanArrayDelegate NT_FreeBooleanArray;
        internal static NT_FreeDoubleArrayDelegate NT_FreeDoubleArray;
        internal static NT_FreeCharArrayDelegate NT_FreeCharArray;
        internal static NT_FreeStringArrayDelegate NT_FreeStringArray;
        internal static NT_GetValueTypeDelegate NT_GetValueType;
        internal static NT_GetValueBooleanDelegate NT_GetValueBoolean;
        internal static NT_GetValueDoubleDelegate NT_GetValueDouble;
        internal static NT_GetValueStringDelegate NT_GetValueString;
        internal static NT_GetValueRawDelegate NT_GetValueRaw;
        internal static NT_GetValueBooleanArrayDelegate NT_GetValueBooleanArray;
        internal static NT_GetValueDoubleArrayDelegate NT_GetValueDoubleArray;
        internal static NT_GetValueStringArrayDelegate NT_GetValueStringArray;
        internal static NT_GetEntryBooleanDelegate NT_GetEntryBoolean;
        internal static NT_GetEntryDoubleDelegate NT_GetEntryDouble;
        internal static NT_GetEntryStringDelegate NT_GetEntryString;
        internal static NT_GetEntryRawDelegate NT_GetEntryRaw;
        internal static NT_GetEntryBooleanArrayDelegate NT_GetEntryBooleanArray;
        internal static NT_GetEntryDoubleArrayDelegate NT_GetEntryDoubleArray;
        internal static NT_GetEntryStringArrayDelegate NT_GetEntryStringArray;
        internal static NT_SetEntryBooleanDelegate NT_SetEntryBoolean;
        internal static NT_SetEntryDoubleDelegate NT_SetEntryDouble;
        internal static NT_SetEntryStringDelegate NT_SetEntryString;
        internal static NT_SetEntryRawDelegate NT_SetEntryRaw;
        internal static NT_SetEntryBooleanArrayDelegate NT_SetEntryBooleanArray;
        internal static NT_SetEntryDoubleArrayDelegate NT_SetEntryDoubleArray;
        internal static NT_SetEntryStringArrayDelegate NT_SetEntryStringArray;
        internal static NT_CreateRpcDelegate NT_CreateRpc;
        internal static NT_CreatePolledRpcDelegate NT_CreatePolledRpc;
        internal static NT_PollRpcDelegate NT_PollRpc;
        internal static NT_PostRpcResponseDelegate NT_PostRpcResponse;
        internal static NT_CallRpcDelegate NT_CallRpc;
        internal static NT_GetRpcResultDelegate NT_GetRpcResult;
    }
}
