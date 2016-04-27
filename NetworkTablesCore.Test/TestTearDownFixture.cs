using NetworkTables.Native;
using NUnit.Framework;

namespace NetworkTablesCore.Test
{
    [SetUpFixture]
    public class TestTearDownFixture
    {
        [TearDown]
        public void TearDown()
        {
            CoreMethods.StopClient();
            CoreMethods.StopServer();

            CoreMethods.StopNotifier();
            CoreMethods.StopRpcServer();
        }
    }
}
