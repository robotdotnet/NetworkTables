using System;
using System.Runtime.InteropServices;

namespace FRC.NetworkTables.Interop
{
    public unsafe class NtCorePInvoke : INtCore
    {
        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeEntryNotificationArray")]
        private static extern void Native_NT_DisposeEntryNotificationArray(NtEntryNotification* arr, UIntPtr count);

        public void NT_DisposeEntryNotificationArray(NtEntryNotification* arr, UIntPtr count)
        {
            Native_NT_DisposeEntryNotificationArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeEntryNotification")]
        private static extern void Native_NT_DisposeEntryNotification(NtEntryNotification* info);

        public void NT_DisposeEntryNotification(NtEntryNotification* info)
        {
            Native_NT_DisposeEntryNotification(info);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeConnectionNotificationArray")]
        private static extern void Native_NT_DisposeConnectionNotificationArray(NtConnectionNotification* arr, UIntPtr count);

        public void NT_DisposeConnectionNotificationArray(NtConnectionNotification* arr, UIntPtr count)
        {
            Native_NT_DisposeConnectionNotificationArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeConnectionNotification")]
        private static extern void Native_NT_DisposeConnectionNotification(NtConnectionNotification* info);

        public void NT_DisposeConnectionNotification(NtConnectionNotification* info)
        {
            Native_NT_DisposeConnectionNotification(info);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeLogMessageArray")]
        private static extern void Native_NT_DisposeLogMessageArray(NtLogMessage* arr, UIntPtr count);

        public void NT_DisposeLogMessageArray(NtLogMessage* arr, UIntPtr count)
        {
            Native_NT_DisposeLogMessageArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeLogMessage")]
        private static extern void Native_NT_DisposeLogMessage(NtLogMessage* info);

        public void NT_DisposeLogMessage(NtLogMessage* info)
        {
            Native_NT_DisposeLogMessage(info);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_Now")]
        private static extern ulong Native_NT_Now();

        public ulong NT_Now()
        {
            return Native_NT_Now();
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreateLoggerPoller")]
        private static extern NtLoggerPoller Native_NT_CreateLoggerPoller(NtInst inst);

        public NtLoggerPoller NT_CreateLoggerPoller(NtInst inst)
        {
            return Native_NT_CreateLoggerPoller(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DestroyLoggerPoller")]
        private static extern void Native_NT_DestroyLoggerPoller(NtLoggerPoller poller);

        public void NT_DestroyLoggerPoller(NtLoggerPoller poller)
        {
            Native_NT_DestroyLoggerPoller(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AddPolledLogger")]
        private static extern NtLogger Native_NT_AddPolledLogger(NtLoggerPoller poller, uint min_level, uint max_level);

        public NtLogger NT_AddPolledLogger(NtLoggerPoller poller, uint min_level, uint max_level)
        {
            return Native_NT_AddPolledLogger(poller, min_level, max_level);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollLogger")]
        private static extern NtLogMessage* Native_NT_PollLogger(NtLoggerPoller poller, UIntPtr* len);

        public NtLogMessage* NT_PollLogger(NtLoggerPoller poller, UIntPtr* len)
        {
            return Native_NT_PollLogger(poller, len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollLoggerTimeout")]
        private static extern NtLogMessage* Native_NT_PollLoggerTimeout(NtLoggerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);

        public NtLogMessage* NT_PollLoggerTimeout(NtLoggerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out)
        {
            return Native_NT_PollLoggerTimeout(poller, len, timeout, timed_out);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CancelPollLogger")]
        private static extern void Native_NT_CancelPollLogger(NtLoggerPoller poller);

        public void NT_CancelPollLogger(NtLoggerPoller poller)
        {
            Native_NT_CancelPollLogger(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_RemoveLogger")]
        private static extern void Native_NT_RemoveLogger(NtLogger logger);

        public void NT_RemoveLogger(NtLogger logger)
        {
            Native_NT_RemoveLogger(logger);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_WaitForLoggerQueue")]
        private static extern NtBool Native_NT_WaitForLoggerQueue(NtInst inst, double timeout);

        public NtBool NT_WaitForLoggerQueue(NtInst inst, double timeout)
        {
            return Native_NT_WaitForLoggerQueue(inst, timeout);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AllocateCharArray")]
        private static extern byte* Native_NT_AllocateCharArray(UIntPtr size);

        public byte* NT_AllocateCharArray(UIntPtr size)
        {
            return Native_NT_AllocateCharArray(size);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AllocateBooleanArray")]
        private static extern NtBool* Native_NT_AllocateBooleanArray(UIntPtr size);

        public NtBool* NT_AllocateBooleanArray(UIntPtr size)
        {
            return Native_NT_AllocateBooleanArray(size);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AllocateDoubleArray")]
        private static extern double* Native_NT_AllocateDoubleArray(UIntPtr size);

        public double* NT_AllocateDoubleArray(UIntPtr size)
        {
            return Native_NT_AllocateDoubleArray(size);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AllocateStringArray")]
        private static extern NtString* Native_NT_AllocateStringArray(UIntPtr size);

        public NtString* NT_AllocateStringArray(UIntPtr size)
        {
            return Native_NT_AllocateStringArray(size);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_FreeCharArray")]
        private static extern void Native_NT_FreeCharArray(byte* v_char);

        public void NT_FreeCharArray(byte* v_char)
        {
            Native_NT_FreeCharArray(v_char);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_FreeDoubleArray")]
        private static extern void Native_NT_FreeDoubleArray(double* v_double);

        public void NT_FreeDoubleArray(double* v_double)
        {
            Native_NT_FreeDoubleArray(v_double);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_FreeBooleanArray")]
        private static extern void Native_NT_FreeBooleanArray(NtBool* v_boolean);

        public void NT_FreeBooleanArray(NtBool* v_boolean)
        {
            Native_NT_FreeBooleanArray(v_boolean);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_FreeStringArray")]
        private static extern void Native_NT_FreeStringArray(NtString* v_string, UIntPtr arr_size);

        public void NT_FreeStringArray(NtString* v_string, UIntPtr arr_size)
        {
            Native_NT_FreeStringArray(v_string, arr_size);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetDefaultInstance")]
        private static extern NtInst Native_NT_GetDefaultInstance();

        public NtInst NT_GetDefaultInstance()
        {
            return Native_NT_GetDefaultInstance();
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreateInstance")]
        private static extern NtInst Native_NT_CreateInstance();

        public NtInst NT_CreateInstance()
        {
            return Native_NT_CreateInstance();
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DestroyInstance")]
        private static extern void Native_NT_DestroyInstance(NtInst inst);

        public void NT_DestroyInstance(NtInst inst)
        {
            Native_NT_DestroyInstance(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetInstanceFromHandle")]
        private static extern NtInst Native_NT_GetInstanceFromHandle(NtHandle handle);

        public NtInst NT_GetInstanceFromHandle(NtHandle handle)
        {
            return Native_NT_GetInstanceFromHandle(handle);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntry")]
        private static extern NtEntry Native_NT_GetEntry(NtInst inst, byte* name, UIntPtr name_len);

        public NtEntry NT_GetEntry(NtInst inst, byte* name, UIntPtr name_len)
        {
            return Native_NT_GetEntry(inst, name, name_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntries")]
        private static extern NtEntry* Native_NT_GetEntries(NtInst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);

        public NtEntry* NT_GetEntries(NtInst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count)
        {
            return Native_NT_GetEntries(inst, prefix, prefix_len, types, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryName")]
        private static extern byte* Native_NT_GetEntryName(NtEntry entry, UIntPtr* name_len);

        public byte* NT_GetEntryName(NtEntry entry, UIntPtr* name_len)
        {
            return Native_NT_GetEntryName(entry, name_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryType")]
        private static extern NtType Native_NT_GetEntryType(NtEntry entry);

        public NtType NT_GetEntryType(NtEntry entry)
        {
            return Native_NT_GetEntryType(entry);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryLastChange")]
        private static extern ulong Native_NT_GetEntryLastChange(NtEntry entry);

        public ulong NT_GetEntryLastChange(NtEntry entry)
        {
            return Native_NT_GetEntryLastChange(entry);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryValue")]
        private static extern void Native_NT_GetEntryValue(NtEntry entry, NtValue* value);

        public void NT_GetEntryValue(NtEntry entry, NtValue* value)
        {
            Native_NT_GetEntryValue(entry, value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetDefaultEntryValue")]
        private static extern NtBool Native_NT_SetDefaultEntryValue(NtEntry entry, NtValue* default_value);

        public NtBool NT_SetDefaultEntryValue(NtEntry entry, NtValue* default_value)
        {
            return Native_NT_SetDefaultEntryValue(entry, default_value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetEntryValue")]
        private static extern NtBool Native_NT_SetEntryValue(NtEntry entry, NtValue* value);

        public NtBool NT_SetEntryValue(NtEntry entry, NtValue* value)
        {
            return Native_NT_SetEntryValue(entry, value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetEntryTypeValue")]
        private static extern void Native_NT_SetEntryTypeValue(NtEntry entry, NtValue* value);

        public void NT_SetEntryTypeValue(NtEntry entry, NtValue* value)
        {
            Native_NT_SetEntryTypeValue(entry, value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetEntryFlags")]
        private static extern void Native_NT_SetEntryFlags(NtEntry entry, uint flags);

        public void NT_SetEntryFlags(NtEntry entry, uint flags)
        {
            Native_NT_SetEntryFlags(entry, flags);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryFlags")]
        private static extern uint Native_NT_GetEntryFlags(NtEntry entry);

        public uint NT_GetEntryFlags(NtEntry entry)
        {
            return Native_NT_GetEntryFlags(entry);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DeleteEntry")]
        private static extern void Native_NT_DeleteEntry(NtEntry entry);

        public void NT_DeleteEntry(NtEntry entry)
        {
            Native_NT_DeleteEntry(entry);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DeleteAllEntries")]
        private static extern void Native_NT_DeleteAllEntries(NtInst inst);

        public void NT_DeleteAllEntries(NtInst inst)
        {
            Native_NT_DeleteAllEntries(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryInfo")]
        private static extern NtEntryInfo* Native_NT_GetEntryInfo(NtInst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);

        public NtEntryInfo* NT_GetEntryInfo(NtInst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count)
        {
            return Native_NT_GetEntryInfo(inst, prefix, prefix_len, types, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetEntryInfoHandle")]
        private static extern NtBool Native_NT_GetEntryInfoHandle(NtEntry entry, NtEntryInfo* info);

        public NtBool NT_GetEntryInfoHandle(NtEntry entry, NtEntryInfo* info)
        {
            return Native_NT_GetEntryInfoHandle(entry, info);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreateEntryListenerPoller")]
        private static extern NtEntryListenerPoller Native_NT_CreateEntryListenerPoller(NtInst inst);

        public NtEntryListenerPoller NT_CreateEntryListenerPoller(NtInst inst)
        {
            return Native_NT_CreateEntryListenerPoller(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DestroyEntryListenerPoller")]
        private static extern void Native_NT_DestroyEntryListenerPoller(NtEntryListenerPoller poller);

        public void NT_DestroyEntryListenerPoller(NtEntryListenerPoller poller)
        {
            Native_NT_DestroyEntryListenerPoller(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AddPolledEntryListener")]
        private static extern NtEntryListener Native_NT_AddPolledEntryListener(NtEntryListenerPoller poller, byte* prefix, UIntPtr prefix_len, uint flags);

        public NtEntryListener NT_AddPolledEntryListener(NtEntryListenerPoller poller, byte* prefix, UIntPtr prefix_len, uint flags)
        {
            return Native_NT_AddPolledEntryListener(poller, prefix, prefix_len, flags);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AddPolledEntryListenerSingle")]
        private static extern NtEntryListener Native_NT_AddPolledEntryListenerSingle(NtEntryListenerPoller poller, NtEntry entry, uint flags);

        public NtEntryListener NT_AddPolledEntryListenerSingle(NtEntryListenerPoller poller, NtEntry entry, uint flags)
        {
            return Native_NT_AddPolledEntryListenerSingle(poller, entry, flags);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollEntryListener")]
        private static extern NtEntryNotification* Native_NT_PollEntryListener(NtEntryListenerPoller poller, UIntPtr* len);

        public NtEntryNotification* NT_PollEntryListener(NtEntryListenerPoller poller, UIntPtr* len)
        {
            return Native_NT_PollEntryListener(poller, len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollEntryListenerTimeout")]
        private static extern NtEntryNotification* Native_NT_PollEntryListenerTimeout(NtEntryListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);

        public NtEntryNotification* NT_PollEntryListenerTimeout(NtEntryListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out)
        {
            return Native_NT_PollEntryListenerTimeout(poller, len, timeout, timed_out);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CancelPollEntryListener")]
        private static extern void Native_NT_CancelPollEntryListener(NtEntryListenerPoller poller);

        public void NT_CancelPollEntryListener(NtEntryListenerPoller poller)
        {
            Native_NT_CancelPollEntryListener(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_RemoveEntryListener")]
        private static extern void Native_NT_RemoveEntryListener(NtEntryListener entry_listener);

        public void NT_RemoveEntryListener(NtEntryListener entry_listener)
        {
            Native_NT_RemoveEntryListener(entry_listener);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_WaitForEntryListenerQueue")]
        private static extern NtBool Native_NT_WaitForEntryListenerQueue(NtInst inst, double timeout);

        public NtBool NT_WaitForEntryListenerQueue(NtInst inst, double timeout)
        {
            return Native_NT_WaitForEntryListenerQueue(inst, timeout);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreateConnectionListenerPoller")]
        private static extern NtConnectionListenerPoller Native_NT_CreateConnectionListenerPoller(NtInst inst);

        public NtConnectionListenerPoller NT_CreateConnectionListenerPoller(NtInst inst)
        {
            return Native_NT_CreateConnectionListenerPoller(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DestroyConnectionListenerPoller")]
        private static extern void Native_NT_DestroyConnectionListenerPoller(NtConnectionListenerPoller poller);

        public void NT_DestroyConnectionListenerPoller(NtConnectionListenerPoller poller)
        {
            Native_NT_DestroyConnectionListenerPoller(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_AddPolledConnectionListener")]
        private static extern NtConnectionListener Native_NT_AddPolledConnectionListener(NtConnectionListenerPoller poller, NtBool immediate_notify);

        public NtConnectionListener NT_AddPolledConnectionListener(NtConnectionListenerPoller poller, NtBool immediate_notify)
        {
            return Native_NT_AddPolledConnectionListener(poller, immediate_notify);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollConnectionListener")]
        private static extern NtConnectionNotification* Native_NT_PollConnectionListener(NtConnectionListenerPoller poller, UIntPtr* len);

        public NtConnectionNotification* NT_PollConnectionListener(NtConnectionListenerPoller poller, UIntPtr* len)
        {
            return Native_NT_PollConnectionListener(poller, len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollConnectionListenerTimeout")]
        private static extern NtConnectionNotification* Native_NT_PollConnectionListenerTimeout(NtConnectionListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);

        public NtConnectionNotification* NT_PollConnectionListenerTimeout(NtConnectionListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out)
        {
            return Native_NT_PollConnectionListenerTimeout(poller, len, timeout, timed_out);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CancelPollConnectionListener")]
        private static extern void Native_NT_CancelPollConnectionListener(NtConnectionListenerPoller poller);

        public void NT_CancelPollConnectionListener(NtConnectionListenerPoller poller)
        {
            Native_NT_CancelPollConnectionListener(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_RemoveConnectionListener")]
        private static extern void Native_NT_RemoveConnectionListener(NtConnectionListener conn_listener);

        public void NT_RemoveConnectionListener(NtConnectionListener conn_listener)
        {
            Native_NT_RemoveConnectionListener(conn_listener);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_WaitForConnectionListenerQueue")]
        private static extern NtBool Native_NT_WaitForConnectionListenerQueue(NtInst inst, double timeout);

        public NtBool NT_WaitForConnectionListenerQueue(NtInst inst, double timeout)
        {
            return Native_NT_WaitForConnectionListenerQueue(inst, timeout);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreateRpcCallPoller")]
        private static extern NtRpcCallPoller Native_NT_CreateRpcCallPoller(NtInst inst);

        public NtRpcCallPoller NT_CreateRpcCallPoller(NtInst inst)
        {
            return Native_NT_CreateRpcCallPoller(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DestroyRpcCallPoller")]
        private static extern void Native_NT_DestroyRpcCallPoller(NtRpcCallPoller poller);

        public void NT_DestroyRpcCallPoller(NtRpcCallPoller poller)
        {
            Native_NT_DestroyRpcCallPoller(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CreatePolledRpc")]
        private static extern void Native_NT_CreatePolledRpc(NtEntry entry, byte* def, UIntPtr def_len, NtRpcCallPoller poller);

        public void NT_CreatePolledRpc(NtEntry entry, byte* def, UIntPtr def_len, NtRpcCallPoller poller)
        {
            Native_NT_CreatePolledRpc(entry, def, def_len, poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollRpc")]
        private static extern NtRpcAnswer* Native_NT_PollRpc(NtRpcCallPoller poller, UIntPtr* len);

        public NtRpcAnswer* NT_PollRpc(NtRpcCallPoller poller, UIntPtr* len)
        {
            return Native_NT_PollRpc(poller, len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PollRpcTimeout")]
        private static extern NtRpcAnswer* Native_NT_PollRpcTimeout(NtRpcCallPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);

        public NtRpcAnswer* NT_PollRpcTimeout(NtRpcCallPoller poller, UIntPtr* len, double timeout, NtBool* timed_out)
        {
            return Native_NT_PollRpcTimeout(poller, len, timeout, timed_out);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CancelPollRpc")]
        private static extern void Native_NT_CancelPollRpc(NtRpcCallPoller poller);

        public void NT_CancelPollRpc(NtRpcCallPoller poller)
        {
            Native_NT_CancelPollRpc(poller);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_WaitForRpcCallQueue")]
        private static extern NtBool Native_NT_WaitForRpcCallQueue(NtInst inst, double timeout);

        public NtBool NT_WaitForRpcCallQueue(NtInst inst, double timeout)
        {
            return Native_NT_WaitForRpcCallQueue(inst, timeout);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PostRpcResponse")]
        private static extern void Native_NT_PostRpcResponse(NtEntry entry, NtRpcCall call, byte* result, UIntPtr result_len);

        public void NT_PostRpcResponse(NtEntry entry, NtRpcCall call, byte* result, UIntPtr result_len)
        {
            Native_NT_PostRpcResponse(entry, call, result, result_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CallRpc")]
        private static extern NtRpcCall Native_NT_CallRpc(NtEntry entry, byte* @params, UIntPtr params_len);

        public NtRpcCall NT_CallRpc(NtEntry entry, byte* @params, UIntPtr params_len)
        {
            return Native_NT_CallRpc(entry, @params, params_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetRpcResult")]
        private static extern byte* Native_NT_GetRpcResult(NtEntry entry, NtRpcCall call, UIntPtr* result_len);

        public byte* NT_GetRpcResult(NtEntry entry, NtRpcCall call, UIntPtr* result_len)
        {
            return Native_NT_GetRpcResult(entry, call, result_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetRpcResultTimeout")]
        private static extern byte* Native_NT_GetRpcResultTimeout(NtEntry entry, NtRpcCall call, UIntPtr* result_len, double timeout, NtBool* timed_out);

        public byte* NT_GetRpcResultTimeout(NtEntry entry, NtRpcCall call, UIntPtr* result_len, double timeout, NtBool* timed_out)
        {
            return Native_NT_GetRpcResultTimeout(entry, call, result_len, timeout, timed_out);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_CancelRpcResult")]
        private static extern void Native_NT_CancelRpcResult(NtEntry entry, NtRpcCall call);

        public void NT_CancelRpcResult(NtEntry entry, NtRpcCall call)
        {
            Native_NT_CancelRpcResult(entry, call);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PackRpcDefinition")]
        private static extern byte* Native_NT_PackRpcDefinition(NtRpcDefinition def, UIntPtr* packed_len);

        public byte* NT_PackRpcDefinition(NtRpcDefinition def, UIntPtr* packed_len)
        {
            return Native_NT_PackRpcDefinition(def, packed_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_UnpackRpcDefinition")]
        private static extern NtBool Native_NT_UnpackRpcDefinition(byte* packed, UIntPtr packed_len, NtRpcDefinition* def);

        public NtBool NT_UnpackRpcDefinition(byte* packed, UIntPtr packed_len, NtRpcDefinition* def)
        {
            return Native_NT_UnpackRpcDefinition(packed, packed_len, def);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_PackRpcValues")]
        private static extern byte* Native_NT_PackRpcValues(NtValue** values, UIntPtr values_len, UIntPtr* packed_len);

        public byte* NT_PackRpcValues(NtValue** values, UIntPtr values_len, UIntPtr* packed_len)
        {
            return Native_NT_PackRpcValues(values, values_len, packed_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_UnpackRpcValues")]
        private static extern NtValue** Native_NT_UnpackRpcValues(byte* packed, UIntPtr packed_len, NtType* types, UIntPtr types_len);

        public NtValue** NT_UnpackRpcValues(byte* packed, UIntPtr packed_len, NtType* types, UIntPtr types_len)
        {
            return Native_NT_UnpackRpcValues(packed, packed_len, types, types_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetNetworkIdentity")]
        private static extern void Native_NT_SetNetworkIdentity(NtInst inst, byte* name, UIntPtr name_len);

        public void NT_SetNetworkIdentity(NtInst inst, byte* name, UIntPtr name_len)
        {
            Native_NT_SetNetworkIdentity(inst, name, name_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetNetworkMode")]
        private static extern uint Native_NT_GetNetworkMode(NtInst inst);

        public uint NT_GetNetworkMode(NtInst inst)
        {
            return Native_NT_GetNetworkMode(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartServer")]
        private static extern void Native_NT_StartServer(NtInst inst, byte* persist_filename, byte* listen_address, uint port);

        public void NT_StartServer(NtInst inst, byte* persist_filename, byte* listen_address, uint port)
        {
            Native_NT_StartServer(inst, persist_filename, listen_address, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StopServer")]
        private static extern void Native_NT_StopServer(NtInst inst);

        public void NT_StopServer(NtInst inst)
        {
            Native_NT_StopServer(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartClientNone")]
        private static extern void Native_NT_StartClientNone(NtInst inst);

        public void NT_StartClientNone(NtInst inst)
        {
            Native_NT_StartClientNone(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartClient")]
        private static extern void Native_NT_StartClient(NtInst inst, byte* server_name, uint port);

        public void NT_StartClient(NtInst inst, byte* server_name, uint port)
        {
            Native_NT_StartClient(inst, server_name, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartClientMulti")]
        private static extern void Native_NT_StartClientMulti(NtInst inst, UIntPtr count, byte** server_names, uint* ports);

        public void NT_StartClientMulti(NtInst inst, UIntPtr count, byte** server_names, uint* ports)
        {
            Native_NT_StartClientMulti(inst, count, server_names, ports);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartClientTeam")]
        private static extern void Native_NT_StartClientTeam(NtInst inst, uint team, uint port);

        public void NT_StartClientTeam(NtInst inst, uint team, uint port)
        {
            Native_NT_StartClientTeam(inst, team, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StopClient")]
        private static extern void Native_NT_StopClient(NtInst inst);

        public void NT_StopClient(NtInst inst)
        {
            Native_NT_StopClient(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetServer")]
        private static extern void Native_NT_SetServer(NtInst inst, byte* server_name, uint port);

        public void NT_SetServer(NtInst inst, byte* server_name, uint port)
        {
            Native_NT_SetServer(inst, server_name, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetServerMulti")]
        private static extern void Native_NT_SetServerMulti(NtInst inst, UIntPtr count, byte** server_names, uint* ports);

        public void NT_SetServerMulti(NtInst inst, UIntPtr count, byte** server_names, uint* ports)
        {
            Native_NT_SetServerMulti(inst, count, server_names, ports);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetServerTeam")]
        private static extern void Native_NT_SetServerTeam(NtInst inst, uint team, uint port);

        public void NT_SetServerTeam(NtInst inst, uint team, uint port)
        {
            Native_NT_SetServerTeam(inst, team, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StartDSClient")]
        private static extern void Native_NT_StartDSClient(NtInst inst, uint port);

        public void NT_StartDSClient(NtInst inst, uint port)
        {
            Native_NT_StartDSClient(inst, port);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_StopDSClient")]
        private static extern void Native_NT_StopDSClient(NtInst inst);

        public void NT_StopDSClient(NtInst inst)
        {
            Native_NT_StopDSClient(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SetUpdateRate")]
        private static extern void Native_NT_SetUpdateRate(NtInst inst, double interval);

        public void NT_SetUpdateRate(NtInst inst, double interval)
        {
            Native_NT_SetUpdateRate(inst, interval);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_Flush")]
        private static extern void Native_NT_Flush(NtInst inst);

        public void NT_Flush(NtInst inst)
        {
            Native_NT_Flush(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_GetConnections")]
        private static extern NtConnectionInfo* Native_NT_GetConnections(NtInst inst, UIntPtr* count);

        public NtConnectionInfo* NT_GetConnections(NtInst inst, UIntPtr* count)
        {
            return Native_NT_GetConnections(inst, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_IsConnected")]
        private static extern NtBool Native_NT_IsConnected(NtInst inst);

        public NtBool NT_IsConnected(NtInst inst)
        {
            return Native_NT_IsConnected(inst);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SavePersistent")]
        private static extern byte* Native_NT_SavePersistent(NtInst inst, byte* filename);

        public byte* NT_SavePersistent(NtInst inst, byte* filename)
        {
            return Native_NT_SavePersistent(inst, filename);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_LoadPersistent")]
        private static extern byte* Native_NT_LoadPersistent(NtInst inst, byte* filename, IntPtr warnFunc);

        public byte* NT_LoadPersistent(NtInst inst, byte* filename, IntPtr warnFunc)
        {
            return Native_NT_LoadPersistent(inst, filename, warnFunc);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_SaveEntries")]
        private static extern byte* Native_NT_SaveEntries(NtInst inst, byte* filename, byte* prefix, UIntPtr prefix_len);

        public byte* NT_SaveEntries(NtInst inst, byte* filename, byte* prefix, UIntPtr prefix_len)
        {
            return Native_NT_SaveEntries(inst, filename, prefix, prefix_len);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_LoadEntries")]
        private static extern byte* Native_NT_LoadEntries(NtInst inst, byte* filename, byte* prefix, UIntPtr prefix_len, IntPtr warnFunc);

        public byte* NT_LoadEntries(NtInst inst, byte* filename, byte* prefix, UIntPtr prefix_len, IntPtr warnFunc)
        {
            return Native_NT_LoadEntries(inst, filename, prefix, prefix_len, warnFunc);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeValue")]
        private static extern void Native_NT_DisposeValue(NtValue* value);

        public void NT_DisposeValue(NtValue* value)
        {
            Native_NT_DisposeValue(value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_InitValue")]
        private static extern void Native_NT_InitValue(NtValue* value);

        public void NT_InitValue(NtValue* value)
        {
            Native_NT_InitValue(value);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeString")]
        private static extern void Native_NT_DisposeString(NtString* str);

        public void NT_DisposeString(NtString* str)
        {
            Native_NT_DisposeString(str);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_InitString")]
        private static extern void Native_NT_InitString(NtString* str);

        public void NT_InitString(NtString* str)
        {
            Native_NT_InitString(str);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeEntryArray")]
        private static extern void Native_NT_DisposeEntryArray(NtEntry* arr, UIntPtr count);

        public void NT_DisposeEntryArray(NtEntry* arr, UIntPtr count)
        {
            Native_NT_DisposeEntryArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeConnectionInfoArray")]
        private static extern void Native_NT_DisposeConnectionInfoArray(NtConnectionInfo* arr, UIntPtr count);

        public void NT_DisposeConnectionInfoArray(NtConnectionInfo* arr, UIntPtr count)
        {
            Native_NT_DisposeConnectionInfoArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeEntryInfoArray")]
        private static extern void Native_NT_DisposeEntryInfoArray(NtEntryInfo* arr, UIntPtr count);

        public void NT_DisposeEntryInfoArray(NtEntryInfo* arr, UIntPtr count)
        {
            Native_NT_DisposeEntryInfoArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeEntryInfo")]
        private static extern void Native_NT_DisposeEntryInfo(NtEntryInfo* info);

        public void NT_DisposeEntryInfo(NtEntryInfo* info)
        {
            Native_NT_DisposeEntryInfo(info);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeRpcDefinition")]
        private static extern void Native_NT_DisposeRpcDefinition(NtRpcDefinition* def);

        public void NT_DisposeRpcDefinition(NtRpcDefinition* def)
        {
            Native_NT_DisposeRpcDefinition(def);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeRpcAnswerArray")]
        private static extern void Native_NT_DisposeRpcAnswerArray(NtRpcAnswer* arr, UIntPtr count);

        public void NT_DisposeRpcAnswerArray(NtRpcAnswer* arr, UIntPtr count)
        {
            Native_NT_DisposeRpcAnswerArray(arr, count);
        }

        [DllImport("ntcorejni", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NT_DisposeRpcAnswer")]
        private static extern void Native_NT_DisposeRpcAnswer(NtRpcAnswer* answer);

        public void NT_DisposeRpcAnswer(NtRpcAnswer* answer)
        {
            Native_NT_DisposeRpcAnswer(answer);
        }

    }
}
