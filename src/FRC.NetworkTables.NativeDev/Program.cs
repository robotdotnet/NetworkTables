using FRC.NetworkTables.Interop;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;

namespace FRC.NetworkTables.NativeDev
{
    class Program
    {
        static string DoT<T>(T value)
        {
            switch (value)
            {
                case bool v:
                    return v.ToString();
                default:
                    return "Not";
            }
        }

        static unsafe void Main(string[] args)
        {
            var s = DoT(false);
            var w = DoT((object)false);
            ;

            string empty = "";
            ReadOnlySpan<char> eSpan = empty.AsSpan();

            fixed (char* p = empty)
            {
                Console.WriteLine(p == null);
            }

            fixed (char* p = eSpan)
            {
                Console.WriteLine(p == null);
            }

            HubConnection connection = null;


            var inst = NetworkTableInstance.Default;
            inst.AddConnectionListener((in ConnectionNotification notify) =>
            {
                Console.WriteLine(notify.Conn.RemoteIp);
                Console.WriteLine(notify.Connected);

            }, false);

            unsafe
            {
                Console.WriteLine(sizeof(NtEntryNotification));
            }

            inst.AddEntryListener("", (in RefEntryNotification notify) =>
            {
                if (notify.Value.Type == NtType.Double)
                {
                   Console.WriteLine($"{notify.Name.ToString()} : {notify.Value.GetDouble()}");
                }

            }, NotifyFlags.Update);

            inst.StartClientTeam(9999);

            var armTable = inst.GetTable("RobotArm");
            


            var stepperEntry = armTable.GetEntry("StepperPosition");

            stepperEntry.SetDefaultBoolean(true);
            stepperEntry.SetBoolean(true);
            stepperEntry.ForceSetString("Hello");

            




            Thread.Sleep(2000);

            foreach (var entry in inst.GetEntries("", 0))
            {
                Console.WriteLine(entry.GetEntryName());
            }

            //inst.
            Console.WriteLine("Hello World!");

            Console.ReadLine();

            //inst.Dispose();
        }
    }
}
