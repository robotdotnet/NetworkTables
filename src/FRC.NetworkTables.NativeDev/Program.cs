using FRC.NetworkTables.Interop;
using System;

namespace FRC.NetworkTables.NativeDev
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!".AsSpan().Slice(3).ToString());

            var inst = NtCore.GetDefaultInstance();
            Console.WriteLine("Hello World!");
        }
    }
}
