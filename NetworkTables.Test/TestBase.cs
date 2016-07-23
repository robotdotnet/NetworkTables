using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Test
{
    internal static class TestBase
    {
        public static void DeleteAllWithPersistent()
        {
            foreach (var entryInfo in NtCore.GetEntryInfo("", 0))
            {
                NtCore.DeleteEntry(entryInfo.Name);
            }
        }
    }
}
