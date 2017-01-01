using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetworkTables.Core.Native;
using NativeLibraryUtilities;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Global

namespace NetworkTables.Core.Test.SpecScanners
{
    public class InteropForTesting
    {
        private InteropForTesting() { }

        static InteropForTesting()
        {
            var nativeLoader = Interop.NativeLoader;

            NativeDelegateInitializer.SetupNativeDelegates<InteropForTesting>(nativeLoader);
        }

#pragma warning disable 649
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetStringForTestingDelegate(byte[] name, ref int size);

        [NativeDelegate]
        internal static NT_GetStringForTestingDelegate NT_GetStringForTesting;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetEntryInfoForTestingDelegate(
            byte[] name, NtType type, uint flags, ulong lastChange, ref int size);
        [NativeDelegate]
        internal static NT_GetEntryInfoForTestingDelegate NT_GetEntryInfoForTesting;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeEntryInfoForTestingDelegate(IntPtr info);
        [NativeDelegate]
        internal static NT_FreeEntryInfoForTestingDelegate NT_FreeEntryInfoForTesting;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetConnectionInfoForTestingDelegate(
            byte[] id, byte[] ip, uint port, ulong lastUpdate, uint protoVersion, ref int size);
        [NativeDelegate]
        internal static NT_GetConnectionInfoForTestingDelegate NT_GetConnectionInfoForTesting;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_FreeConnectionInfoForTestingDelegate(IntPtr info);
        [NativeDelegate]
        internal static NT_FreeConnectionInfoForTestingDelegate NT_FreeConnectionInfoForTesting;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr NT_GetRpcCallInfoForTestingDelegate(
            uint rpcId, uint callUid, byte[] name, byte[] param, UIntPtr paramLen, ref int size);
        [NativeDelegate]
        internal static NT_GetRpcCallInfoForTestingDelegate NT_GetRpcCallInfoForTesting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeRpcCallInfoDelegate(ref NtRpcCallInfo callInfo);
        [NativeDelegate]
        internal static NT_DisposeRpcCallInfoDelegate NT_DisposeRpcCallInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NT_DisposeRpcCallInfoIntPtrDelegate(IntPtr callInfo);
        [NativeDelegate("NT_DisposeRpcCallInfo")]
        internal static NT_DisposeRpcCallInfoIntPtrDelegate NT_DisposeRpcCallInfoIntPtr;

#pragma warning restore 649
    }
}
