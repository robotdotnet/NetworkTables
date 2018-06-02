using FRC.NetworkTables.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace FRC.NetworkTables.Core.Test
{
    public class EntryListenerTest
    {
        private void Connect(NetworkTableInstance server, NetworkTableInstance client)
        {
            server.SetNetworkIdentity("server");
            client.SetNetworkIdentity("client");

            server.StartServer("entrylistenertest.ini", "127.0.0.1", 10000);
            client.StartClient("127.0.0.1", 10000);

            var poller = NtCore.CreateConnectionListenerPoller(client.Handle);
            NtCore.AddPolledConnectionListener(poller, false);
            Assert.NotEqual(0, NtCore.PollConnectionListenerTimeout(client, poller, 1.0, out var timedOut, Span<ConnectionNotification>.Empty).Length);
        }

        [Fact]
        public void TestPrefixNewRemote()
        {
            using (var server = NetworkTableInstance.Create())
            using (var client = NetworkTableInstance.Create())
            {
                Connect(server, client);
                List<EntryNotification> events = new List<EntryNotification>();
                var handle = server.AddEntryListener("/foo", (in EntryNotification n) =>
                {
                    events.Add(n);
                }, NotifyFlags.New);

                client.GetEntry("/foo/bar").SetDouble(1.0);
                client.GetEntry("/baz").SetDouble(1.0);
                client.Flush();
                Thread.Sleep(100);

                Assert.True(server.WaitForEntryListenerQueue(1.0));

                Assert.Single(events);
                Assert.Equal(handle, events[0].Listener);
                Assert.Equal(server.GetEntry("/foo/bar"), events[0].Entry);
                Assert.Equal("/foo/bar", events[0].Name);
                Assert.Equal(NetworkTableValue.MakeDouble(1.0), events[0].Value);
                Assert.Equal(NotifyFlags.New, events[0].Flags);
            }
        }
    }
}
