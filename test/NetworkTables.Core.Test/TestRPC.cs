using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class RpcTest : TestBase
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            NtCore.SetLogger((level, file, line, msg) =>
            {
                Console.WriteLine(msg);
            }, LogLevel.LogInfo);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            NtCore.StopRpcServer();
        }

        [Test]
        public void TestPolledRpc()
        {
            Console.WriteLine("TestPolledRpc");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            RemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            RpcCallInfo info;
            bool polled = RemoteProcedureCall.PollRpc(true, TimeSpan.FromSeconds(1), out info);
            Assert.That(polled, Is.True);

            IReadOnlyList<byte> toSendBack = Callback1(info.Name, info.Params, new ConnectionInfo());
            Assert.That(toSendBack.Count, Is.Not.EqualTo(0));

            RemoteProcedureCall.PostRpcResponse(info.RpcId, info.CallUid, toSendBack);

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestPolledRpcTimeout()
        {
            Console.WriteLine("TestPolledRpcTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            RemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");
            TimeSpan timeoutTime = TimeSpan.FromSeconds(1);
            Stopwatch sw = Stopwatch.StartNew();
            RpcCallInfo info;
            bool polled = RemoteProcedureCall.PollRpc(true, timeoutTime, out info);
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
            RemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            RpcCallInfo info;
            bool polled = RemoteProcedureCall.PollRpc(false, out info);
            Assert.That(polled, Is.False);
        }

        [Test]
        public void TestPolledRpcNonBlockingWithTimeout()
        {
            Console.WriteLine("TestPolledRpcNonBlockingWithTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            RemoteProcedureCall.CreatePolledRpc("func1", def);

            Console.WriteLine("Calling RPC");

            RpcCallInfo info;
            bool polled = RemoteProcedureCall.PollRpc(false, TimeSpan.FromSeconds(1), out info);
            Assert.That(polled, Is.False);
        }

        private static IReadOnlyList<byte> Callback1(string names, IReadOnlyList<byte> paramsStr, ConnectionInfo connInfo)
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
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeed()
        {
            Console.WriteLine("TestRpcSpeed");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                IReadOnlyList<byte> call1Result = null;
                RemoteProcedureCall.GetRpcResult(true, call1Uid, out call1Result);
                var res = RemoteProcedureCall.UnpackRpcValues(call1Result, new[] { NtType.Double });
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
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            TimeSpan timeoutTime = TimeSpan.FromSeconds(1);
            Stopwatch sw = Stopwatch.StartNew();
            IReadOnlyList<byte> result = null;
            bool success = RemoteProcedureCall.GetRpcResult(true, call1Uid, timeoutTime, out result);
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
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            bool success = RemoteProcedureCall.GetRpcResult(false, call1Uid, out result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestRpcLocalNonBlockingWithTimeout()
        {
            Console.WriteLine("LocalNonBlockingWithTimeout");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = 585;

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            bool success = RemoteProcedureCall.GetRpcResult(false, call1Uid, TimeSpan.FromSeconds(1), out result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestRpcLocalTimeoutSuccess()
        {
            Console.WriteLine("LocalTimeoutSuccess");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, TimeSpan.FromSeconds(1), out result);
            Assert.That(result, Is.Not.Null);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedTimeoutSuccess()
        {
            Console.WriteLine("SpeedTimeoutSuccess");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                IReadOnlyList<byte> call1Result = null;
                RemoteProcedureCall.GetRpcResult(true, call1Uid, TimeSpan.FromSeconds(1), out call1Result);
                Assert.That(call1Result, Is.Not.Null);
                var res = RemoteProcedureCall.UnpackRpcValues(call1Result, new[] { NtType.Double });
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
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));

            Console.WriteLine("Waiting for RPC Result");
            CancellationTokenSource source = new CancellationTokenSource();
            var task = RemoteProcedureCall.GetRpcResultAsync(call1Uid, source.Token);
            var completed = task.Wait(TimeSpan.FromSeconds(1));
            if (!completed) source.Cancel();
            Assert.That(completed, Is.True);
            Assert.That(task.IsCompleted);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedAsync()
        {
            Console.WriteLine("SpeedAsync");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                CancellationTokenSource source = new CancellationTokenSource();
                var task = RemoteProcedureCall.GetRpcResultAsync(call1Uid, source.Token);
                var completed = task.Wait(TimeSpan.FromSeconds(1));
                if (!completed) source.Cancel();
                Assert.That(completed, Is.True);
                Assert.That(task.IsCompleted);
                var res = RemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
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
            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");
            Console.WriteLine("Waiting for RPC Result");
            CancellationTokenSource source = new CancellationTokenSource();
            var task = RemoteProcedureCall.CallRpcWithResultAsync("func1", source.Token, RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(2.0) }));
            var completed = task.Wait(TimeSpan.FromSeconds(1));
            if (!completed) source.Cancel();
            Assert.That(completed, Is.True);
            Assert.That(task.IsCompleted);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeedAsyncSingleCall()
        {
            Console.WriteLine("SpeedAsyncSingleCall");
            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                CancellationTokenSource source = new CancellationTokenSource();
                var task = RemoteProcedureCall.CallRpcWithResultAsync("func1", source.Token, RemoteProcedureCall.PackRpcValues(new[] { Value.MakeDouble(i) }));
                var completed = task.Wait(TimeSpan.FromSeconds(1));
                if (!completed) source.Cancel();
                Assert.That(completed, Is.True);
                Assert.That(task.IsCompleted);
                var res = RemoteProcedureCall.UnpackRpcValues(task.Result, new[] { NtType.Double });
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static IReadOnlyList<byte> Callback2(string names, IReadOnlyList<byte> paramsStr, ConnectionInfo connInfo)
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

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback2);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(new[] {Value.MakeBoolean(true), Value.MakeBooleanArray(new[] { true, false }), Value.MakeDouble(2.2), Value.MakeDoubleArray(new[] { 2.8, 6.87 }),
                Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 }), Value.MakeString("NewString"), Value.MakeStringArray(new[] { "String1", "String2" })}));

            Console.WriteLine("Waiting for RPC Result");
            IReadOnlyList<byte> result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, new[] { NtType.Boolean, NtType.BooleanArray, NtType.Double, NtType.DoubleArray, NtType.Raw, NtType.String, NtType.StringArray });
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }
    }
}
