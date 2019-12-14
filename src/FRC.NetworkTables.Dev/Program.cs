using FRC.NetworkTables.Interop;
using System;

namespace FRC.NetworkTables.Dev
{
    class Program
    {
        static void Main(string[] args)
        {

            NetworkTableInstance inst = NetworkTableInstance.Default;
            foreach (var s in inst.GetTable("SmartDashboard").GetSubTables())
            {
                ;
            }

            Console.WriteLine("Hello World!");
        }
    }
}
