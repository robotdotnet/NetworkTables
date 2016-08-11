using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestRPC : TestBase
    {
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            CoreMethods.StopRpcServer();
        }

        private byte[] Callback1(string names, byte[] paramsStr)
        {
            var param = RemoteProcedureCall.UnpackRpcValues(paramsStr, NtType.Double);

            if (param.Count == 0)
            {
                Console.Error.WriteLine("Empty Params?");
                return new byte[] { 0 };
            }
            double val = param[0].GetDouble();
            //Console.WriteLine($"Called with {val}");

            return RemoteProcedureCall.PackRpcValues(Value.MakeDouble(val + 1.2));
        }

        [Test]
        public void TestRpcLocal()
        {

            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);

            Console.WriteLine("Calling RPC");

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(Value.MakeDouble(2.0)));

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, NtType.Double);
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }

        [Test]
        public void TestRpcSpeed()
        {

            var def = new RpcDefinition(1, "myfunc1", new List<RpcParamDef> { new RpcParamDef("param1", Value.MakeDouble(0.0)) }, new List<RpcResultsDef> { new RpcResultsDef("result1", NtType.Double) });

            RemoteProcedureCall.CreateRpc("func1", RemoteProcedureCall.PackRpcDefinition(def), Callback1);



            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] call1Result = null;
            for (int i = 0; i < 10000; ++i)
            {
                long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(Value.MakeDouble(i)));
                RemoteProcedureCall.GetRpcResult(true, call1Uid, out call1Result);
                var res = RemoteProcedureCall.UnpackRpcValues(call1Result, NtType.Double);
                Assert.AreNotEqual(0, res.Count, "RPC Result empty");
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

        }

















        private byte[] Callback2(string names, byte[] paramsStr)
        {
            var param = RemoteProcedureCall.UnpackRpcValues(paramsStr, NtType.Boolean, NtType.BooleanArray, NtType.Double, NtType.DoubleArray, NtType.Raw, NtType.String, NtType.StringArray);

            if (param.Count == 0)
            {
                Console.Error.WriteLine("Empty Params?");
                return new byte[] { 0 };
            }

            return RemoteProcedureCall.PackRpcValues(Value.MakeBoolean(true), Value.MakeBooleanArray(new[] { true, false }), Value.MakeDouble(2.2), Value.MakeDoubleArray(new[] { 2.8, 6.876 }),
                Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 }), Value.MakeString("NewString"), Value.MakeStringArray(new[] { "String1", "String2" }));
        }

        [Test]
        public void TestRpcAllTypes()
        {

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

            long call1Uid = RemoteProcedureCall.CallRpc("func1", RemoteProcedureCall.PackRpcValues(Value.MakeBoolean(true), Value.MakeBooleanArray(new[] { true, false }), Value.MakeDouble(2.2), Value.MakeDoubleArray(new[] { 2.8, 6.87 }),
                Value.MakeRaw(new byte[] { 52, 0, 89, 0, 0, 98 }), Value.MakeString("NewString"), Value.MakeStringArray(new[] { "String1", "String2" })));

            Console.WriteLine("Waiting for RPC Result");
            byte[] result = null;
            RemoteProcedureCall.GetRpcResult(true, call1Uid, out result);
            var call1Result = RemoteProcedureCall.UnpackRpcValues(result, NtType.Boolean, NtType.BooleanArray, NtType.Double, NtType.DoubleArray, NtType.Raw, NtType.String, NtType.StringArray);
            Assert.AreNotEqual(0, call1Result.Count, "RPC Result empty");

            Console.WriteLine(call1Result[0].ToString());
        }
    }


}
