using System;
using NetworkTables;
using NUnit.Framework;

namespace NetworkTablesCore.Test
{
    public class TestBase
    {
        public static bool s_started = false;

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            if (!s_started)
            {
                Console.WriteLine(Environment.Is64BitProcess ? "Tests running in 64 Bit mode" : "Tests running in 32 Bit mode.");
                NetworkTable.SetIPAddress("127.0.0.1");
                NetworkTable.SetPort(10000);
                NetworkTable.SetServerMode();
                NetworkTable.Initialize();
                s_started = true;
            }
        }
    }
}
