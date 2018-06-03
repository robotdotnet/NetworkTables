using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRC.NetworkTables.Core.Web
{
    public class NTHub : Hub
    {
        private readonly NetworkTableInstance m_instance;

        public NTHub(NetworkTableInstance instance)
        {
            m_instance = instance;

            instance.AddConnectionListener((in ConnectionNotification notification) =>
            {
                var n = (
                    notification.Conn,
                    notification.Connected
                );
                Task.Run(async () =>
                {
                    await Clients.All.SendAsync("ConnectionNotification", (object)n.Conn, n.Connected);
                });
            }, false);

            instance.AddEntryListener("", (in RefEntryNotification notification) =>
            {
                var n =
                (
                    Name: notification.Name.ToString(),
                    Value: notification.Value.Value.GetValue(),
                    notification.Flags
                );
                Task.Run(async () =>
                {
                    await Clients.All.SendAsync("EntryNotification", n.Name, n.Value, n.Flags);
                });
            }, (NotifyFlags)0xFFFFFFFFu);
        }

        public EntryFlags GetFlags(string key)
        {
            return m_instance.GetEntry(key).GetEntryFlags();
        }

        public void SetFlags(string key, EntryFlags flags)
        {
            m_instance.GetEntry(key).SetFlags(flags);
        }

        public object GetValue(string key)
        {
            return m_instance.GetEntry(key).GetObjectValue();
        }

        public bool SetValue(string key, object value)
        {
            return m_instance.GetEntry(key).SetValue(value);
        }

        public void ForceSetValue(string key, object value)
        {
            m_instance.GetEntry(key).ForceSetValue(value);
        }

        public bool SetDefaultValue(string key, object value)
        {
            return m_instance.GetEntry(key).SetDefaultValue(value);
        }


    }
}
