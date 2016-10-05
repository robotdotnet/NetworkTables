using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Independent;
using NUnit.Framework;

namespace NetworkTables.Test.ConnectionTesting
{
    [TestFixture]
    public class RpcRemoteTest
    {
        private IndependentNtCore serverNtCore;
        private IndependentRemoteProcedureCall serverRemoteProcedureCall;

        private IndependentNtCore clientNtCore;
        private IndependentRemoteProcedureCall clientRemoteProcedureCall;

        const int Port = 5578;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            serverNtCore = new IndependentNtCore();
            clientNtCore = new IndependentNtCore();

            serverRemoteProcedureCall = new IndependentRemoteProcedureCall(serverNtCore);
            clientRemoteProcedureCall = new IndependentRemoteProcedureCall(clientNtCore);

            serverNtCore.RemoteName = "Server";
            clientNtCore.RemoteName = "Client";

            serverNtCore.StartServer("rpcPersist.txt", "", Port);
            clientNtCore.StartClient("localhost", Port);

            serverNtCore.UpdateRate = 0.01;
            clientNtCore.UpdateRate = 0.01;

            int count = 0;
            while (true)
            {
                var conns = serverNtCore.GetConnections();
                if (conns.Count > 0) break;
                Thread.Sleep(100);
                if (count > 10)
                {
                    Assert.Fail("Could not connect");
                    return;
                }
                count++;
            }
            Thread.Sleep(500);
            // Everything is connected
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            clientNtCore.Dispose();
            serverNtCore.Dispose();
        }

        [Test]
        public void TestPolledRpc()
        {
            Console.WriteLine("TestPolledRpc");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreatePolledRpc("func1", def);
            Thread.Sleep(30);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));
            Thread.Sleep(20);

            RpcCallInfo info;
            bool polled = serverRemoteProcedureCall.PollRpc(true, TimeSpan.FromSeconds(1), out info);
            Assert.That(polled, Is.True);

            IList<byte> toSendBack = Callback1(info.Name, info.Params, new ConnectionInfo());
            Assert.That(toSendBack.Count, Is.Not.EqualTo(0));

            serverRemoteProcedureCall.PostRpcResponse(info.RpcId, info.CallUid, toSendBack);

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            clientRemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }


        [Test]
        public void TestPolledRpcAsync()
        {
            Console.WriteLine("TestPolledRpcAsync");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            CancellationTokenSource source = new CancellationTokenSource();
            var task = serverRemoteProcedureCall.PollRpcAsync(source.Token);
            bool completed = task.Wait(TimeSpan.FromSeconds(1));
            if (!completed) source.Cancel();
            Assert.That(completed, Is.True);
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.Not.Null);
            Assert.That(task.Result.HasValue);

            IList<byte> toSendBack = Callback1(task.Result.Value.Name, task.Result.Value.Params, new ConnectionInfo());
            Assert.That(toSendBack.Count, Is.Not.EqualTo(0));

            serverRemoteProcedureCall.PostRpcResponse(task.Result.Value.RpcId, task.Result.Value.CallUid, toSendBack);

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            clientRemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }


        [Test]
        public void TestPolledRpcTimeout()
        {
            Console.WriteLine("TestPolledRpcTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");
            TimeSpan timeoutTime = TimeSpan.FromSeconds(1);
            Stopwatch sw = Stopwatch.StartNew();
            RpcCallInfo info;
            bool polled = serverRemoteProcedureCall.PollRpc(true, timeoutTime, out info);
            sw.Stop();
            Assert.That(polled, Is.False);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.25);
            Assert.That(sw.Elapsed < timeoutTime + tolerance);
            Assert.That(sw.Elapsed > timeoutTime - tolerance);
        }

        [Test]
        public void TestPolledRpcNonBlocking()
        {
            Console.WriteLine("TestPolledRpcNonBlocking");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            RpcCallInfo info;
            bool polled = serverRemoteProcedureCall.PollRpc(false, out info);
            Assert.That(polled, Is.False);
        }

        [Test]
        public void TestPolledRpcNonBlockingWithTimeout()
        {
            Console.WriteLine("TestPolledRpcNonBlockingWithTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            RpcCallInfo info;
            bool polled = serverRemoteProcedureCall.PollRpc(false, TimeSpan.FromSeconds(1), out info);
            Assert.That(polled, Is.False);
        }

        private static IList<byte> Callback1(string names, byte[] paramsStr, ConnectionInfo connInfo)
        {
            var param = RemoteProcedureCall.UnpackRpcValues(paramsStr, new[] { NtType.Double });

            if (param.Count == 0)
            {
                Console.Error.WriteLine("Empty Params?");
                return new byte[] { 0 };
            }
            double val = param[0].GetDouble();

            return RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(val + 1.2) });
        }

        [Test]
        public void TestRpcLocal()
        {
            Console.WriteLine("TestRpcLocal");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            clientRemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeed()
        {
            Console.WriteLine("TestRpcSpeed");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 20; ++i)
            {
                long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                byte[] call1Result = null;
                clientRemoteProcedureCall.GetRpcResult(true, call1Uid, out call1Result);
                var res = clientRemoteProcedureCall.UnpackRpcValues(call1Result, new[] { NtType.Double });
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

        }

        [Test]
        public void TestRpcLocalTimeoutFailure()
        {
            Console.WriteLine("LocalTimeoutFailure");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            TimeSpan timeoutTime = TimeSpan.FromSeconds(1);
            Stopwatch sw = Stopwatch.StartNew();
            byte[] result = null;
            bool success = clientRemoteProcedureCall.GetRpcResult(true, call1Uid, timeoutTime, out result);
            sw.Stop();
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.25);
            Assert.That(sw.Elapsed < timeoutTime + tolerance);
            Assert.That(sw.Elapsed > timeoutTime - tolerance);
        }

        [Test]
        public void TestRpcLocalNonBlocking()
        {
            Console.WriteLine("LoclaNonBlocking");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            bool success = clientRemoteProcedureCall.GetRpcResult(false, call1Uid, out result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestRpcLocalNonBlockingWithTimeout()
        {
            Console.WriteLine("LocalNonBlockingWithTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            bool success = clientRemoteProcedureCall.GetRpcResult(false, call1Uid, TimeSpan.FromSeconds(1), out result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestRpcLocalTimeoutSuccess()
        {
            Console.WriteLine("LocalTimeoutSuccess");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            clientRemoteProcedureCall.GetRpcResult(true, call1Uid, TimeSpan.FromSeconds(1), out result);
            Assert.That(result, Is.Not.Null);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedTimeoutSuccess()
        {
            Console.WriteLine("SpeedTimeoutSuccess");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 20; ++i)
            {
                long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                byte[] call1Result = null;
                clientRemoteProcedureCall.GetRpcResult(true, call1Uid, TimeSpan.FromSeconds(1), out call1Result);
                Assert.That(call1Result, Is.Not.Null);
                var res = clientRemoteProcedureCall.UnpackRpcValues(call1Result, new[] { NtType.Double });
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

        }

        [Test]
        public void TestRpcLocalAsync()
        {
            Console.WriteLine("LocalAsync");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            CancellationTokenSource source = new CancellationTokenSource();
            var task = clientRemoteProcedureCall.GetRpcResultAsync(call1Uid, source.Token);
            var completed = task.Wait(TimeSpan.FromSeconds(1));
            if (!completed) source.Cancel();
            Assert.That(completed, Is.True);
            Assert.That(task.IsCompleted);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedAsync()
        {
            Console.WriteLine("SpeedAsync");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 20; ++i)
            {
                long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                CancellationTokenSource source = new CancellationTokenSource();
                var task = clientRemoteProcedureCall.GetRpcResultAsync(call1Uid, source.Token);
                var completed = task.Wait(TimeSpan.FromSeconds(1));
                if (!completed) source.Cancel();
                Assert.That(completed, Is.True);
                Assert.That(task.IsCompleted);
                var res = clientRemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        [Test]
        public void TestRpcLocalAsyncSingleCall()
        {
            Console.WriteLine("LocalAsyncSingleCall");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");
            Console.WriteLine("Waiting for RPC Result");
            CancellationTokenSource source = new CancellationTokenSource();
            var task = clientRemoteProcedureCall.CallRpcWithResultAsync("func1", source.Token, clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));
            var completed = task.Wait(TimeSpan.FromSeconds(1));
            if (!completed) source.Cancel();
            Assert.That(completed, Is.True);
            Assert.That(task.IsCompleted);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedAsyncSingleCall()
        {
            Console.WriteLine("SpeedAsyncSingleCall");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 20; ++i)
            {
                CancellationTokenSource source = new CancellationTokenSource();
                var task = clientRemoteProcedureCall.CallRpcWithResultAsync("func1", source.Token, clientRemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                var completed = task.Wait(TimeSpan.FromSeconds(1));
                if (!completed) source.Cancel();
                Assert.That(completed, Is.True);
                Assert.That(task.IsCompleted);
                var res = clientRemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static IList<byte> Callback2(string names, byte[] paramsStr, ConnectionInfo connInfo)
        {
            var param = RemoteProcedureCall.UnpackRpcValues(paramsStr, new[] { NtType.Boolean, NtType.BooleanArray, NtType.Double, NtType.DoubleArray, NtType.Raw, NtType.String, NtType.StringArray });

            if (param.Count == 0)
            {
                Console.Error.WriteLine("Empty Params?");
                return new byte[] { 0 };
            }

            return RemoteProcedureCall.PackRpcValues(new[] {Value.MakeBoolean(true), Value.MakeBooleanArray(new[] { true, false }), Value.MakeDouble(2.2), Value.MakeDoubleArray(new[] { 2.8, 6.876 }),
                Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 }), Value.MakeString("NewString"), Value.MakeStringArray(new[] { "String1", "String2" }) });
        }

        [Test]
        public void TestRpcAllTypes()
        {
            Console.WriteLine("AllTypes");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef>
            {
                new RpcParamDef("param1", Value.MakeBoolean(true)),
                new RpcParamDef("param2", Value.MakeBooleanArray(new[] { true, false })),
                new RpcParamDef("param3", Value.MakeDouble(0.0)),
                new RpcParamDef("param4", Value.MakeDoubleArray(new[] { 2.8, 6.87 })),
                new RpcParamDef("param5", Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 })),
                new RpcParamDef("param6", Value.MakeString("NewString")),
                new RpcParamDef("param7", Value.MakeStringArray(new[] { "String1", "String2" })),
            }, new List<RpcResultsDef>
            {
                new RpcResultsDef("result1", NtType.Boolean),
                new RpcResultsDef("result2", NtType.BooleanArray),
                new RpcResultsDef("result3", NtType.Double),
                new RpcResultsDef("result4", NtType.DoubleArray),
                new RpcResultsDef("result5", NtType.Raw),
                new RpcResultsDef("result6", NtType.String),
                new RpcResultsDef("result7", NtType.StringArray),
            });

            serverRemoteProcedureCall.CreateRpc("func1", serverRemoteProcedureCall.PackRpcDefinition(def), Callback2);

            Console.WriteLine("Calling RPC");

            long call1Uid = clientRemoteProcedureCall.CallRpc("func1", clientRemoteProcedureCall.PackRpcValues(new[] {Value.MakeBoolean(true), Value.MakeBooleanArray(new[] { true, false }), Value.MakeDouble(2.2), Value.MakeDoubleArray(new[] { 2.8, 6.87 }),
                Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 }), Value.MakeString("NewString"), Value.MakeStringArray(new[] { "String1", "String2" })}));

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            clientRemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = clientRemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Boolean, NtType.BooleanArray, NtType.Double, NtType.DoubleArray, NtType.Raw, NtType.String, NtType.StringArray });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }
    }
}
