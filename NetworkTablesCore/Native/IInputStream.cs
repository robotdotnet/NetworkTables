using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Native
{
    internal interface IInputStream
    {
        bool Read(byte[] data, int len);
        void Close();


    }
}
