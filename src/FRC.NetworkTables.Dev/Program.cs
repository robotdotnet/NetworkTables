using FRC.NetworkTables.Interop;
using System;

namespace FRC.NetworkTables.Dev
{
    class Program
    {
        static void Main(string[] args)
        {
            var inst = NtCore.GetDefaultInstance();
            Console.WriteLine("Hello World!");
        }
    }
}
