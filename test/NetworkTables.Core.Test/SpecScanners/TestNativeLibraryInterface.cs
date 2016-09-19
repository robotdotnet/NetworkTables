using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NetworkTables.Core.Native;
using NetworkTables.Core.NativeLibraryUtilities;
using NUnit.Framework;
using static NetworkTables.Core.Test.SpecScanners.InteropForTesting;
using static NetworkTables.Core.Native.CoreMethods;
using static NetworkTables.Core.Native.Interop;

namespace NetworkTables.Core.Test.SpecScanners
{
    [TestFixture]
    public class TestNativeLibraryInterface
    {
#if !NETCOREAPP1_0
        public struct HALDelegateClass
        {
            public string ClassName;
            public List<DelegateDeclarationSyntax> Methods;
        }

        // Code for this found here
        // http://stackoverflow.com/questions/19001423/getting-path-of-a-to-the-parent-folder-of-the-solution-file-c-sharp
        private static string FindRootSolutionDirectory()
        {
#if NETCOREAPP1_0
            return null
#else
            var assembly = Assembly.GetExecutingAssembly();
            var p = Path.DirectorySeparatorChar;
            var path = assembly.CodeBase.Replace("file:///", "").Replace("/", p.ToString());
            path = Path.GetDirectoryName(path);


            var directory = new DirectoryInfo(
            path ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory?.FullName;
#endif
        }

        // Gets a list of all of our delegates used by the HAL
        public static List<HALDelegateClass> GetDelegates()
        {
            List<HALDelegateClass> halBaseMethods = new List<HALDelegateClass>();

            var pathToSolution = FindRootSolutionDirectory();
            var p = Path.DirectorySeparatorChar;
            Assert.That(pathToSolution, Is.Not.Null);
            var file = $"{pathToSolution}{p}src{p}FRC.NetworkTables.Core{p}Native{p}Interop.cs";
            HALDelegateClass cs = new HALDelegateClass
            {
                ClassName = "",
                Methods = new List<DelegateDeclarationSyntax>()
            };
            using (var stream = File.OpenRead(file))
            {
                var tree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: file);
                cs.ClassName = Path.GetFileName(file);
                var methods =
                    tree.GetRoot()
                        .DescendantNodes()
                        .OfType<DelegateDeclarationSyntax>()
                        .Select(a => a).ToList();
                cs.Methods.AddRange(methods);
            }
            halBaseMethods.Add(cs);

            return halBaseMethods;
        }


        /// <summary>
        /// Gets a list of all the native symbols needed by HAL-RoboRIO
        /// </summary>
        /// <returns></returns>
        public static List<string> GetRequestedNativeSymbols()
        {
            List<string> nativeFunctions = new List<string>();
            var pathToSolution = FindRootSolutionDirectory();
            var p = Path.DirectorySeparatorChar;
            Assert.That(pathToSolution, Is.Not.Null);
            var dir = $"{pathToSolution}{p}src{p}FRC.NetworkTables.Core{p}Native";
            foreach (var file in Directory.GetFiles(dir, "*.cs"))
            {
                if (!file.ToLower().Contains("Interop")) continue;
                using (StreamReader reader = new StreamReader(file))
                {
                    bool foundInitialize = false;
                    string line;
                    while (!foundInitialize)
                    {
                        line = reader.ReadLine();
                        if (line == null) break;
                        if (line.ToLower().Contains("static void initializedelegates")) foundInitialize = true;
                    }
                    if (!foundInitialize) continue;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.TrimStart(' ').StartsWith("//")) continue;
                        if (line.ToLower().Contains("marshal.getdelegateforfunctionpointer"))
                        {
                            int startParam = line.IndexOf('"');
                            int endParam = line.IndexOf('"', startParam + 1);
                            nativeFunctions.Add(line.Substring(startParam + 1, endParam - startParam - 1));
                        }
                    }
                }
            }
            return nativeFunctions;
        }


        [Test]
        public void TestRoboRioMapsToNativeAssemblySymbols()
        {
            OsType type = NativeLibraryLoader.GetOsType();

            //Only run the roboRIO symbol test on windows.
            if (type != OsType.Windows32 && type != OsType.Windows64) Assert.Pass();

            var roboRIOSymbols = GetRequestedNativeSymbols();

            var pathToSolution = FindRootSolutionDirectory();
            var ps = Path.DirectorySeparatorChar;
            Assert.That(pathToSolution, Is.Not.Null);
            var dirToNetworkTablesLib = $"{pathToSolution}{ps}src{ps}FRC.NetworkTables.Core";

            // Start the child process.
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = $"{dirToNetworkTablesLib}\\NativeLibraries\\roborio\\frcnm.exe";
            Console.WriteLine(p.StartInfo.FileName);
            p.StartInfo.Arguments = $"{dirToNetworkTablesLib}\\NativeLibraries\\roborio\\libntcore.so";
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            bool found = true;


            string[] nativeSymbols = output.Split('\r');

            foreach (var halSymbol in roboRIOSymbols)
            {
                bool foundSymbol = nativeSymbols.Any(nativeSymbol => nativeSymbol.EndsWith(halSymbol));
                if (!foundSymbol)
                {
                    found = false;
                    Console.WriteLine(halSymbol);
                }
            }

            Assert.That(found);
        }
#endif

        //Checks all our types for blittable
        private void CheckForBlittable(List<TypeSyntax> types, List<string> allowedTypes, List<string> nonBlittableFuncs, string nonBlittableLine)
        {
            bool allBlittable = true;
            foreach (TypeSyntax t in types)
            {
                bool found = false;
                foreach (string a in allowedTypes)
                {
                    if (t.ToString().Contains(a)) //Contains is OK, since arrays of blittable types are blittable.
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    allBlittable = false;
                }
            }

            if (!allBlittable)
            {
                nonBlittableFuncs.Add(nonBlittableLine);
            }
        }

        private static bool IsBlittable(Type type)
        {
            // If is array
            if (type.IsArray)
            {
                //Check that the elements are value type, and that the element itself is blittable.
                var elements = type.GetElementType();
                return elements.GetTypeInfo().IsValueType && IsBlittable(elements);
            }
            try
            {
                //Otherwise try and pin the type. If it pins, it is blittable.
                //If exception is thrown, it is not blittable, and do not allow.
#if NETCOREAPP1_0
                object obj = Activator.CreateInstance(type);
#else
                object obj = FormatterServices.GetUninitializedObject(type);
#endif
                GCHandle.Alloc(obj, GCHandleType.Pinned).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        [Test]
        public void TestNtConnectionInfo()
        {
            string name = "Testing";
            string ip = "localhost";
            uint port = 1756;
            ulong lastUpdate = 26;
            uint protoRev = 0x0300;
            int nativeSize = 0;

            UIntPtr nameLen;
            byte[] nameArr = CreateUTF8String(name, out nameLen);
            UIntPtr ipLen;
            byte[] ipArr = CreateUTF8String(ip, out ipLen);
            IntPtr connectionInfoPtr = NT_GetConnectionInfoForTesting(nameArr, ipArr, port, lastUpdate, protoRev,
                ref nativeSize);
            Assert.That(Marshal.SizeOf(typeof(NtConnectionInfo)), Is.EqualTo(nativeSize));
            Assert.That(IsBlittable(typeof(NtConnectionInfo)));

            using (NtStringWrite nameToWrite = new NtStringWrite(name))
            using (NtStringWrite ipToWrite = new NtStringWrite(ip))
            {


                NtConnectionInfo managedInfo = new NtConnectionInfo(new NtStringRead(nameToWrite.str, nameToWrite.len),
                    new NtStringRead(ipToWrite.str, ipToWrite.len), port, lastUpdate, protoRev);

                Assert.That(managedInfo.RemoteId.ToString(), Is.EqualTo(name));
                Assert.That(managedInfo.RemoteIp.ToString(), Is.EqualTo(ip));
                Assert.That(managedInfo.RemotePort, Is.EqualTo(port));
                Assert.That(managedInfo.LastUpdate, Is.EqualTo(lastUpdate));
                Assert.That(managedInfo.ProtocolVersion, Is.EqualTo(protoRev));


                List<byte> nativeArray = new List<byte>();
                List<byte> managedArray = new List<byte>();

                int bytesToSkip = (Marshal.SizeOf(typeof(IntPtr)) == 8) ? 32 : 16;
                unsafe
                {
                    NtConnectionInfo* connInfo = (NtConnectionInfo*)connectionInfoPtr.ToPointer();
                    Assert.That(connInfo->RemoteId.ToString(), Is.EqualTo(name));
                    Assert.That(connInfo->RemoteIp.ToString(), Is.EqualTo(ip));
                    Assert.That(connInfo->RemotePort, Is.EqualTo(port));
                    Assert.That(connInfo->LastUpdate, Is.EqualTo(lastUpdate));
                    Assert.That(connInfo->ProtocolVersion, Is.EqualTo(protoRev));



                    byte* bp = (byte*)connInfo;
                    for (int i = bytesToSkip; i < nativeSize; i++)
                    {
                        nativeArray.Add(bp[i]);
                    }

                    NtConnectionInfo* managedConn = &managedInfo;
                    byte* mbp = (byte*)managedConn;

                    for (int i = bytesToSkip; i < Marshal.SizeOf(typeof(NtConnectionInfo)); i++)
                    {
                        managedArray.Add(mbp[i]);
                    }
                }

                // Assert that everything past our pointer values are equivelent
                Assert.That(nativeArray, Is.EquivalentTo(managedArray));

            }

            NT_FreeConnectionInfoForTesting(connectionInfoPtr);
        }

        [Test]
        public void TestNtStringRead()
        {
            int numberNonChangingBytes = 0;
            int numberPointers = 2;
            int pointerSize = Marshal.SizeOf(typeof(IntPtr));

            int pointerTotal = numberPointers * pointerSize;
            Assert.That(Marshal.SizeOf(typeof(NtStringRead)), Is.EqualTo(pointerTotal + numberNonChangingBytes));
            Assert.That(IsBlittable(typeof(NtStringRead)));
        }

        [Test]
        public void TestNtStringWrite()
        {
            int numberNonChangingBytes = 0;
            int numberPointers = 2;
            int pointerSize = Marshal.SizeOf(typeof(IntPtr));

            int pointerTotal = numberPointers * pointerSize;
            Assert.That(Marshal.SizeOf(typeof(NtStringWrite)), Is.EqualTo(pointerTotal + numberNonChangingBytes));
            Assert.That(IsBlittable(typeof(NtStringWrite)));
        }

        [Test]
        public void TestNtRpcCallInfo()
        {
            uint rpcId = 10549;
            uint callUid = 8085;
            string name = "TestName";
            string param = "Params\0Others";

            UIntPtr nameLen;
            byte[] nameArr = CreateUTF8String(name, out nameLen);
            UIntPtr paramLen;
            byte[] paramArr = CreateUTF8String(param, out paramLen);
            int nativeSize = 0;

            IntPtr rpcCallInfoPtr = NT_GetRpcCallInfoForTesting(rpcId, callUid, nameArr, paramArr, paramLen, ref nativeSize);



            Assert.That(Marshal.SizeOf(typeof(NtRpcCallInfo)), Is.EqualTo(nativeSize));
            Assert.That(IsBlittable(typeof(NtRpcCallInfo)));

            using (NtStringWrite nameToWrite = new NtStringWrite(name))
            using (NtStringWrite paramToWrite = new NtStringWrite(param))
            {


                NtRpcCallInfo managedInfo = new NtRpcCallInfo(new NtStringRead(nameToWrite.str, nameToWrite.len),
                    new NtStringRead(paramToWrite.str, paramToWrite.len), rpcId, callUid);

                Assert.That(managedInfo.Name.ToString(), Is.EqualTo(name));
                Assert.That(managedInfo.Param.ToString(), Is.EqualTo(param));
                Assert.That(managedInfo.CallUid, Is.EqualTo(callUid));
                Assert.That(managedInfo.RpcId, Is.EqualTo(rpcId));


                List<byte> nativeArray = new List<byte>();
                List<byte> managedArray = new List<byte>();

                int bytesToSkip = (Marshal.SizeOf(typeof(IntPtr)) == 8) ? 32 : 16;
                unsafe
                {
                    NtRpcCallInfo* callInfo = (NtRpcCallInfo*)rpcCallInfoPtr.ToPointer();
                    Assert.That(callInfo->Name.ToString(), Is.EqualTo(name));
                    Assert.That(callInfo->Param.ToString(), Is.EqualTo(param));
                    Assert.That(callInfo->CallUid, Is.EqualTo(callUid));
                    Assert.That(callInfo->RpcId, Is.EqualTo(rpcId));



                    byte* bp = (byte*)callInfo;
                    for (int i = 0; i < nativeSize - bytesToSkip; i++)
                    {
                        nativeArray.Add(bp[i]);
                    }

                    NtRpcCallInfo* managedRpc = &managedInfo;
                    byte* mbp = (byte*)managedRpc;

                    for (int i = 0; i < Marshal.SizeOf(typeof(NtRpcCallInfo)) - bytesToSkip; i++)
                    {
                        managedArray.Add(mbp[i]);
                    }
                }

                // Assert that everything past our pointer values are equivelent
                Assert.That(nativeArray, Is.EquivalentTo(managedArray));

            }

            NT_DisposeRpcCallInfoIntPtr(rpcCallInfoPtr);
        }

        [Test]
        public void TestNtStringWriteArray()
        {
            object obj = new NtStringWrite[6];
            Assert.DoesNotThrow(() => GCHandle.Alloc(obj, GCHandleType.Pinned).Free());
        }
#if !NETCOREAPP1_0
        [Test]
        public void TestNativeIntefaceBlittable()
        {
            OsType type = NativeLibraryLoader.GetOsType();
            //Only run the blittable test on windows. Pathing on linux is being an issue
            if (type != OsType.Windows32 && type != OsType.Windows64) Assert.Pass();

            List<string> allowedTypes = new List<string>()
            {
                // Allowed types with arrays are also allowed
                "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "IntPtr", "UIntPtr", "float", "void", "double",

                // For now force our enum types to be OK
                "NtType",
                //"CTR_Code", "HALAccelerometerRange", "HALAllianceStationID", "AnalogTriggerType", "Mode",

                //Allow delegates to be blittable
                "WarmFunction", "NT_LogFunc", "NT_ConnectionListenerCallback", "NT_EntryListenerCallback", "NT_RPCCallback",

                //Also allow any structs known to be blittable
                "NtStringRead", "NtStringWrite", "NtConnectionInfo", "NtRpcCallInfo",

                //For now allow bool, since it marshalls easily
                //This will change if the native windows HAL is not 1 byte bools
                "bool",
            };

            List<string> notBlittableMethods = new List<string>();


            var halBaseDelegates = GetDelegates();
            foreach (var halDelegate in halBaseDelegates)
            {
                foreach (var methodSyntax in halDelegate.Methods)
                {
                    List<TypeSyntax> types = new List<TypeSyntax>();

                    if (methodSyntax.AttributeLists.Count != 0)
                    {
                        types.Add(methodSyntax.ReturnType);
                    }
                    else
                    {
                        types.Add(methodSyntax.ReturnType);
                    }

                    List<string> param = new List<string>();

                    StringBuilder builder = new StringBuilder();
                    builder.Append($"\t {methodSyntax.ReturnType} {methodSyntax.Identifier} (");
                    bool first = true;
                    foreach (var parameter in methodSyntax.ParameterList.Parameters)
                    {
                        if (parameter.AttributeLists.Count != 0)
                        {
                            types.Add(parameter.Type);
                        }
                        else
                        {
                            types.Add(parameter.Type);
                        }


                        param.Add(parameter.Type.ToString());
                        if (first)
                        {
                            first = false;
                            builder.Append($"{parameter.Type} {parameter.Identifier}");
                        }
                        else
                        {
                            builder.Append($", {parameter.Type} {parameter.Identifier}");
                        }
                    }
                    builder.Append(")");

                    CheckForBlittable(types, allowedTypes, notBlittableMethods, builder.ToString());
                }
            }

            foreach (string s in notBlittableMethods)
            {
                Console.WriteLine(s);
            }

            if (notBlittableMethods.Count != 0)
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }
#endif
    }

}
