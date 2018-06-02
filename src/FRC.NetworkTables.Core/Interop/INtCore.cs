using AdvancedDLSupport.AOT;
using System;

namespace FRC.NetworkTables.Interop
{
    public interface INtCore
    {
        unsafe NT_Inst NT_GetDefaultInstance();
        unsafe NT_Inst NT_CreateInstance();
        unsafe void NT_DestroyInstance(NT_Inst inst);
        unsafe NT_Inst NT_GetInstanceFromHandle(NT_Handle handle);




        unsafe NT_Entry NT_GetEntry(NT_Inst inst, byte* name, UIntPtr name_len);




        unsafe NT_Entry* NT_GetEntries(NT_Inst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);




        unsafe byte* NT_GetEntryName(NT_Entry entry, UIntPtr* name_len);




        unsafe NtType NT_GetEntryType(NT_Entry entry);




        unsafe ulong NT_GetEntryLastChange(NT_Entry entry);




        unsafe void NT_GetEntryValue(NT_Entry entry, NT_Value* value);




        unsafe NT_Bool NT_SetDefaultEntryValue(NT_Entry entry, NT_Value* default_value);




        unsafe NT_Bool NT_SetEntryValue(NT_Entry entry, NT_Value* value);




        unsafe void NT_SetEntryTypeValue(NT_Entry entry, NT_Value* value);




        unsafe void NT_SetEntryFlags(NT_Entry entry, uint flags);




        unsafe uint NT_GetEntryFlags(NT_Entry entry);




        unsafe void NT_DeleteEntry(NT_Entry entry);




        unsafe void NT_DeleteAllEntries(NT_Inst inst);




        unsafe NT_EntryInfo* NT_GetEntryInfo(NT_Inst inst, byte* prefix, UIntPtr prefix_len, uint types, UIntPtr* count);




        unsafe NT_Bool NT_GetEntryInfoHandle(NT_Entry entry, NT_EntryInfo* info);




        unsafe NT_EntryListenerPoller NT_CreateEntryListenerPoller(NT_Inst inst);




        unsafe void NT_DestroyEntryListenerPoller(NT_EntryListenerPoller poller);




        unsafe NT_EntryListener NT_AddPolledEntryListener(NT_EntryListenerPoller poller, byte* prefix, UIntPtr prefix_len, uint flags);




        unsafe NT_EntryListener NT_AddPolledEntryListenerSingle(NT_EntryListenerPoller poller, NT_Entry entry, uint flags);




        unsafe NT_EntryNotification* NT_PollEntryListener(NT_EntryListenerPoller poller, UIntPtr* len);




        unsafe NT_EntryNotification* NT_PollEntryListenerTimeout(NT_EntryListenerPoller poller, UIntPtr* len, double timeout, NT_Bool* timed_out);




        unsafe void NT_CancelPollEntryListener(NT_EntryListenerPoller poller);




        unsafe void NT_RemoveEntryListener(NT_EntryListener entry_listener);




        unsafe NT_Bool NT_WaitForEntryListenerQueue(NT_Inst inst, double timeout);




        unsafe NT_ConnectionListenerPoller NT_CreateConnectionListenerPoller(NT_Inst inst);




        unsafe void NT_DestroyConnectionListenerPoller(NT_ConnectionListenerPoller poller);




        unsafe NT_ConnectionListener NT_AddPolledConnectionListener(NT_ConnectionListenerPoller poller, NT_Bool immediate_notify);




        unsafe NT_ConnectionNotification* NT_PollConnectionListener(NT_ConnectionListenerPoller poller, UIntPtr* len);




        unsafe NT_ConnectionNotification* NT_PollConnectionListenerTimeout(NT_ConnectionListenerPoller poller, UIntPtr* len, double timeout, NT_Bool* timed_out);




        unsafe void NT_CancelPollConnectionListener(NT_ConnectionListenerPoller poller);




        unsafe void NT_RemoveConnectionListener(NT_ConnectionListener conn_listener);




        unsafe NT_Bool NT_WaitForConnectionListenerQueue(NT_Inst inst, double timeout);




        unsafe NT_RpcCallPoller NT_CreateRpcCallPoller(NT_Inst inst);




        unsafe void NT_DestroyRpcCallPoller(NT_RpcCallPoller poller);




        unsafe void NT_CreatePolledRpc(NT_Entry entry, byte* def, UIntPtr def_len, NT_RpcCallPoller poller);




        unsafe NT_RpcAnswer* NT_PollRpc(NT_RpcCallPoller poller, UIntPtr* len);




        unsafe NT_RpcAnswer* NT_PollRpcTimeout(NT_RpcCallPoller poller, UIntPtr* len, double timeout, NT_Bool* timed_out);




        unsafe void NT_CancelPollRpc(NT_RpcCallPoller poller);




        unsafe NT_Bool NT_WaitForRpcCallQueue(NT_Inst inst, double timeout);




        unsafe void NT_PostRpcResponse(NT_Entry entry, NT_RpcCall call, byte* result, UIntPtr result_len);




        unsafe NT_RpcCall NT_CallRpc(NT_Entry entry, byte* @params, UIntPtr params_len);




        unsafe byte* NT_GetRpcResult(NT_Entry entry, NT_RpcCall call, UIntPtr* result_len);




        unsafe byte* NT_GetRpcResultTimeout(NT_Entry entry, NT_RpcCall call, UIntPtr* result_len, double timeout, NT_Bool* timed_out);




        unsafe void NT_CancelRpcResult(NT_Entry entry, NT_RpcCall call);




        unsafe byte* NT_PackRpcDefinition(NT_RpcDefinition def, UIntPtr* packed_len);




        unsafe NT_Bool NT_UnpackRpcDefinition(byte* packed, UIntPtr packed_len, NT_RpcDefinition* def);




        unsafe byte* NT_PackRpcValues(NT_Value** values, UIntPtr values_len, UIntPtr* packed_len);




        unsafe NT_Value** NT_UnpackRpcValues(byte* packed, UIntPtr packed_len, NtType* types, UIntPtr types_len);




        unsafe void NT_SetNetworkIdentity(NT_Inst inst, byte* name, UIntPtr name_len);




        unsafe uint NT_GetNetworkMode(NT_Inst inst);




        unsafe void NT_StartServer(NT_Inst inst, byte* persist_filename, byte* listen_address, uint port);




        unsafe void NT_StopServer(NT_Inst inst);




        unsafe void NT_StartClientNone(NT_Inst inst);




        unsafe void NT_StartClient(NT_Inst inst, byte* server_name, uint port);




        unsafe void NT_StartClientMulti(NT_Inst inst, UIntPtr count, byte** server_names, uint* ports);




        unsafe void NT_StartClientTeam(NT_Inst inst, uint team, uint port);




        unsafe void NT_StopClient(NT_Inst inst);




        unsafe void NT_SetServer(NT_Inst inst, byte* server_name, uint port);




        unsafe void NT_SetServerMulti(NT_Inst inst, UIntPtr count, byte** server_names, uint* ports);




        unsafe void NT_SetServerTeam(NT_Inst inst, uint team, uint port);




        unsafe void NT_StartDSClient(NT_Inst inst, uint port);




        unsafe void NT_StopDSClient(NT_Inst inst);




        unsafe void NT_SetUpdateRate(NT_Inst inst, double interval);




        unsafe void NT_Flush(NT_Inst inst);




        unsafe NT_ConnectionInfo* NT_GetConnections(NT_Inst inst, UIntPtr* count);




        unsafe NT_Bool NT_IsConnected(NT_Inst inst);




        unsafe byte* NT_SavePersistent(NT_Inst inst, byte* filename);




        unsafe byte* NT_LoadPersistent(NT_Inst inst, byte* filename, IntPtr warnFunc);




        unsafe byte* NT_SaveEntries(NT_Inst inst, byte* filename, byte* prefix, UIntPtr prefix_len);




        unsafe byte* NT_LoadEntries(NT_Inst inst, byte* filename, byte* prefix, UIntPtr prefix_len, IntPtr warnFunc);




        unsafe void NT_DisposeValue(NT_Value* value);




        unsafe void NT_InitValue(NT_Value* value);




        unsafe void NT_DisposeString(NT_String* str);




        unsafe void NT_InitString(NT_String* str);




        unsafe void NT_DisposeEntryArray(NT_Entry* arr, UIntPtr count);




        unsafe void NT_DisposeConnectionInfoArray(NT_ConnectionInfo* arr, UIntPtr count);




        unsafe void NT_DisposeEntryInfoArray(NT_EntryInfo* arr, UIntPtr count);




        unsafe void NT_DisposeEntryInfo(NT_EntryInfo* info);




        unsafe void NT_DisposeRpcDefinition(NT_RpcDefinition* def);




        unsafe void NT_DisposeRpcAnswerArray(NT_RpcAnswer* arr, UIntPtr count);




        unsafe void NT_DisposeRpcAnswer(NT_RpcAnswer* answer);




        unsafe void NT_DisposeEntryNotificationArray(NT_EntryNotification* arr, UIntPtr count);




        unsafe void NT_DisposeEntryNotification(NT_EntryNotification* info);




        unsafe void NT_DisposeConnectionNotificationArray(NT_ConnectionNotification* arr, UIntPtr count);




        unsafe void NT_DisposeConnectionNotification(NT_ConnectionNotification* info);




        unsafe void NT_DisposeLogMessageArray(NT_LogMessage* arr, UIntPtr count);




        unsafe void NT_DisposeLogMessage(NT_LogMessage* info);




        unsafe ulong NT_Now();




        unsafe NT_LoggerPoller NT_CreateLoggerPoller(NT_Inst inst);




        unsafe void NT_DestroyLoggerPoller(NT_LoggerPoller poller);




        unsafe NT_Logger NT_AddPolledLogger(NT_LoggerPoller poller, uint min_level, uint max_level);




        unsafe NT_LogMessage* NT_PollLogger(NT_LoggerPoller poller, UIntPtr* len);




        unsafe NT_LogMessage* NT_PollLoggerTimeout(NT_LoggerPoller poller, UIntPtr* len, double timeout, NT_Bool* timed_out);




        unsafe void NT_CancelPollLogger(NT_LoggerPoller poller);




        unsafe void NT_RemoveLogger(NT_Logger logger);




        unsafe NT_Bool NT_WaitForLoggerQueue(NT_Inst inst, double timeout);




        unsafe byte* NT_AllocateCharArray(UIntPtr size);




        unsafe NT_Bool* NT_AllocateBooleanArray(UIntPtr size);




        unsafe double* NT_AllocateDoubleArray(UIntPtr size);




        unsafe NT_String* NT_AllocateStringArray(UIntPtr size);




        unsafe void NT_FreeCharArray(byte* v_char);




        unsafe void NT_FreeDoubleArray(double* v_double);




        unsafe void NT_FreeBooleanArray(NT_Bool* v_boolean);




        unsafe void NT_FreeStringArray(NT_String* v_string, UIntPtr arr_size);


    }
}
    
