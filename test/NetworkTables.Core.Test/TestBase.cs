using System;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    public class TestBase
    {
        public static void DeleteAllWithPersistent()
        {
            foreach (var entryInfo in NtCore.GetEntryInfo("", 0))
            {
                NtCore.DeleteEntry(entryInfo.Name);
            }
        }

        public static bool s_started = false;

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            if (!s_started)
            {
                Console.WriteLine(IntPtr.Size != sizeof(int) ? "Tests running in 64 Bit mode" : "Tests running in 32 Bit mode.");
                NetworkTable.SetIPAddress("127.0.0.1");
                NetworkTable.SetPort(8912);
                NetworkTable.SetServerMode();
                NetworkTable.Initialize();
                s_started = true;
            }
        }
    }
}
