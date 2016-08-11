using System;
using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [SetUpFixture]
    public class TestTearDownFixture
    {
        [OneTimeTearDown]
        public void TearDown()
        {
#if false
            Console.WriteLine("Stopping Clietn");
            CoreMethods.StopClient();
            Console.WriteLine("Stopping Server");
            CoreMethods.StopServer();
#endif
            Console.WriteLine("Stopping Notifier");
            CoreMethods.StopNotifier();
            Console.WriteLine("Stoping RpcServer");
            CoreMethods.StopRpcServer();
        }
    }
}
