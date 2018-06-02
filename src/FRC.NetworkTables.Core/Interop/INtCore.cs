using AdvancedDLSupport.AOT;
using System;

namespace FRC.NetworkTables.Interop
{
    public interface INtCore
    {
        unsafe Instance NT_GetDefaultInstance();
        unsafe Instance NT_CreateInstance();
        unsafe void NT_DestroyInstance(Instance inst);
        unsafe Instance NT_GetInstanceFromHandle(Handle handle);




        unsafe Entry NT_GetEntry(Instance inst, byte* name, UIntPtr name_len);




        unsafe Entry* NT_GetEntries(Instance inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);




        unsafe byte* NT_GetEntryName(Entry entry, UIntPtr* name_len);




        unsafe NtType NT_GetEntryType(Entry entry);




        unsafe ulong NT_GetEntryLastChange(Entry entry);




        unsafe void NT_GetEntryValue(Entry entry, NtValue* value);




        unsafe NtBool NT_SetDefaultEntryValue(Entry entry, NtValue* default_value);




        unsafe NtBool NT_SetEntryValue(Entry entry, NtValue* value);




        unsafe void NT_SetEntryTypeValue(Entry entry, NtValue* value);




        unsafe void NT_SetEntryFlags(Entry entry, uint flags);




        unsafe uint NT_GetEntryFlags(Entry entry);




        unsafe void NT_DeleteEntry(Entry entry);




        unsafe void NT_DeleteAllEntries(Instance inst);




        unsafe NtEntryInfo* NT_GetEntryInfo(Instance inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);




        unsafe NtBool NT_GetEntryInfoHandle(Entry entry, NtEntryInfo* info);




        unsafe EntryListenerPoller NT_CreateEntryListenerPoller(Instance inst);




        unsafe void NT_DestroyEntryListenerPoller(EntryListenerPoller poller);




        unsafe EntryListener NT_AddPolledEntryListener(EntryListenerPoller poller, byte* prefix, UIntPtr prefix_len, uint flags);




        unsafe EntryListener NT_AddPolledEntryListenerSingle(EntryListenerPoller poller, Entry entry, uint flags);




        unsafe NtEntryNotification* NT_PollEntryListener(EntryListenerPoller poller, UIntPtr* len);




        unsafe NtEntryNotification* NT_PollEntryListenerTimeout(EntryListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);




        unsafe void NT_CancelPollEntryListener(EntryListenerPoller poller);




        unsafe void NT_RemoveEntryListener(EntryListener entry_listener);




        unsafe NtBool NT_WaitForEntryListenerQueue(Instance inst, double timeout);




        unsafe ConnectionListenerPoller NT_CreateConnectionListenerPoller(Instance inst);




        unsafe void NT_DestroyConnectionListenerPoller(ConnectionListenerPoller poller);




        unsafe ConnectionListener NT_AddPolledConnectionListener(ConnectionListenerPoller poller, NtBool immediate_notify);




        unsafe NtConnectionNotification* NT_PollConnectionListener(ConnectionListenerPoller poller, UIntPtr* len);




        unsafe NtConnectionNotification* NT_PollConnectionListenerTimeout(ConnectionListenerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);




        unsafe void NT_CancelPollConnectionListener(ConnectionListenerPoller poller);




        unsafe void NT_RemoveConnectionListener(ConnectionListener conn_listener);




        unsafe NtBool NT_WaitForConnectionListenerQueue(Instance inst, double timeout);




        unsafe RpcCallPoller NT_CreateRpcCallPoller(Instance inst);




        unsafe void NT_DestroyRpcCallPoller(RpcCallPoller poller);




        unsafe void NT_CreatePolledRpc(Entry entry, byte* def, UIntPtr def_len, RpcCallPoller poller);




        unsafe NtRpcAnswer* NT_PollRpc(RpcCallPoller poller, UIntPtr* len);




        unsafe NtRpcAnswer* NT_PollRpcTimeout(RpcCallPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);




        unsafe void NT_CancelPollRpc(RpcCallPoller poller);




        unsafe NtBool NT_WaitForRpcCallQueue(Instance inst, double timeout);




        unsafe void NT_PostRpcResponse(Entry entry, RpcCall call, byte* result, UIntPtr result_len);




        unsafe RpcCall NT_CallRpc(Entry entry, byte* @params, UIntPtr params_len);




        unsafe byte* NT_GetRpcResult(Entry entry, RpcCall call, UIntPtr* result_len);




        unsafe byte* NT_GetRpcResultTimeout(Entry entry, RpcCall call, UIntPtr* result_len, double timeout, NtBool* timed_out);




        unsafe void NT_CancelRpcResult(Entry entry, RpcCall call);




        unsafe byte* NT_PackRpcDefinition(NtRpcDefinition def, UIntPtr* packed_len);




        unsafe NtBool NT_UnpackRpcDefinition(byte* packed, UIntPtr packed_len, NtRpcDefinition* def);




        unsafe byte* NT_PackRpcValues(NtValue** values, UIntPtr values_len, UIntPtr* packed_len);




        unsafe NtValue** NT_UnpackRpcValues(byte* packed, UIntPtr packed_len, NtType* types, UIntPtr types_len);




        unsafe void NT_SetNetworkIdentity(Instance inst, byte* name, UIntPtr name_len);




        unsafe uint NT_GetNetworkMode(Instance inst);




        unsafe void NT_StartServer(Instance inst, byte* persist_filename, byte* listen_address, uint port);




        unsafe void NT_StopServer(Instance inst);




        unsafe void NT_StartClientNone(Instance inst);




        unsafe void NT_StartClient(Instance inst, byte* server_name, uint port);




        unsafe void NT_StartClientMulti(Instance inst, UIntPtr count, byte** server_names, uint* ports);




        unsafe void NT_StartClientTeam(Instance inst, uint team, uint port);




        unsafe void NT_StopClient(Instance inst);




        unsafe void NT_SetServer(Instance inst, byte* server_name, uint port);




        unsafe void NT_SetServerMulti(Instance inst, UIntPtr count, byte** server_names, uint* ports);




        unsafe void NT_SetServerTeam(Instance inst, uint team, uint port);




        unsafe void NT_StartDSClient(Instance inst, uint port);




        unsafe void NT_StopDSClient(Instance inst);




        unsafe void NT_SetUpdateRate(Instance inst, double interval);




        unsafe void NT_Flush(Instance inst);




        unsafe NtConnectionInfo* NT_GetConnections(Instance inst, UIntPtr* count);




        unsafe NtBool NT_IsConnected(Instance inst);




        unsafe byte* NT_SavePersistent(Instance inst, byte* filename);




        unsafe byte* NT_LoadPersistent(Instance inst, byte* filename, IntPtr warnFunc);




        unsafe byte* NT_SaveEntries(Instance inst, byte* filename, byte* prefix, UIntPtr prefix_len);




        unsafe byte* NT_LoadEntries(Instance inst, byte* filename, byte* prefix, UIntPtr prefix_len, IntPtr warnFunc);




        unsafe void NT_DisposeValue(NtValue* value);




        unsafe void NT_InitValue(NtValue* value);




        unsafe void NT_DisposeString(NtString* str);




        unsafe void NT_InitString(NtString* str);




        unsafe void NT_DisposeEntryArray(Entry* arr, UIntPtr count);




        unsafe void NT_DisposeConnectionInfoArray(NtConnectionInfo* arr, UIntPtr count);




        unsafe void NT_DisposeEntryInfoArray(NtEntryInfo* arr, UIntPtr count);




        unsafe void NT_DisposeEntryInfo(NtEntryInfo* info);




        unsafe void NT_DisposeRpcDefinition(NtRpcDefinition* def);




        unsafe void NT_DisposeRpcAnswerArray(NtRpcAnswer* arr, UIntPtr count);




        unsafe void NT_DisposeRpcAnswer(NtRpcAnswer* answer);




        unsafe void NT_DisposeEntryNotificationArray(NtEntryNotification* arr, UIntPtr count);




        unsafe void NT_DisposeEntryNotification(NtEntryNotification* info);




        unsafe void NT_DisposeConnectionNotificationArray(NtConnectionNotification* arr, UIntPtr count);




        unsafe void NT_DisposeConnectionNotification(NtConnectionNotification* info);




        unsafe void NT_DisposeLogMessageArray(NtLogMessage* arr, UIntPtr count);




        unsafe void NT_DisposeLogMessage(NtLogMessage* info);




        unsafe ulong NT_Now();




        unsafe LoggerPoller NT_CreateLoggerPoller(Instance inst);




        unsafe void NT_DestroyLoggerPoller(LoggerPoller poller);




        unsafe Logger NT_AddPolledLogger(LoggerPoller poller, uint min_level, uint max_level);




        unsafe NtLogMessage* NT_PollLogger(LoggerPoller poller, UIntPtr* len);




        unsafe NtLogMessage* NT_PollLoggerTimeout(LoggerPoller poller, UIntPtr* len, double timeout, NtBool* timed_out);




        unsafe void NT_CancelPollLogger(LoggerPoller poller);




        unsafe void NT_RemoveLogger(Logger logger);




        unsafe NtBool NT_WaitForLoggerQueue(Instance inst, double timeout);




        unsafe byte* NT_AllocateCharArray(UIntPtr size);




        unsafe NtBool* NT_AllocateBooleanArray(UIntPtr size);




        unsafe double* NT_AllocateDoubleArray(UIntPtr size);




        unsafe NtString* NT_AllocateStringArray(UIntPtr size);




        unsafe void NT_FreeCharArray(byte* v_char);




        unsafe void NT_FreeDoubleArray(double* v_double);




        unsafe void NT_FreeBooleanArray(NtBool* v_boolean);




        unsafe void NT_FreeStringArray(NtString* v_string, UIntPtr arr_size);


    }
}
    
