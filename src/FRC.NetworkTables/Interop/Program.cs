/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FRC.NetworkTables.Core.Interop
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            List<string> fpNames = new List<string>();

            List<string> ilFile = new List<string>();

            string opening = ".class public abstract auto ansi sealed beforefieldinit ";
            opening += typeof(Functions).FullName;
            opening += " extends [System.Runtime]System.Object";

            ilFile.Add(opening);
            ilFile.Add("{");
            ilFile.Add("");



            var methods = typeof(Functions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var method in methods)
            {
                fpNames.Add($"{method.Name}FunctionPointer");

                Console.WriteLine($"public static IntPtr {method.Name}FunctionPointer;");


                ilFile.Add($"  .field public static native int {method.Name}FunctionPointer");
                
            }

            ilFile.Add("");

            string test = ConvertTypeToIL(typeof(NT_Value*));
            ;

            foreach(var method in methods)
            {
                ilFile.Add("");
                string decl = "  .method public hidebysic static " + ConvertTypeToIL(method.ReturnType) + " " + method.Name + "(";

                string parameterList = "";

                var parameters = method.GetParameters();
                
                foreach(var para in parameters)
                {
                    parameterList += ConvertTypeToIL(para.ParameterType) + " '" + para.Name + "', ";
                }

                if (parameterList.Length > 0)
                {
                    parameterList = parameterList.Substring(0, parameterList.Length - 2);
                }

                decl += parameterList + ") cil managed aggressiveinlining";

                ilFile.Add(decl);
                ilFile.Add("  {");
                ilFile.Add($"    .maxstack {parameters.Length + 1}");

                int i = 0;
                for (i = 0; i < 4 && i < parameters.Length; i++)
                {
                    // Less then 4, use mimimal arg loaders
                    ilFile.Add($"    ldarg.{i}");
                }
                for (;i<parameters.Length; i++)
                {
                    ilFile.Add($"    ldarg {i}");
                }

                ilFile.Add($"    ldsfld native int {typeof(Functions).FullName}::{method.Name}FunctionPointer");
                string calli = $"    calli unmanaged cdecl {ConvertTypeToIL(method.ReturnType)}({parameterList})";
                ilFile.Add(calli);
                ilFile.Add("    ret");

                
                //ilFile.Add("    ")


                ilFile.Add("  }");

            }

            ilFile.Add("}");



            File.WriteAllLines("ILCode.il", ilFile);
            ;
        }
        public static string ConvertTypeToIL(Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }
            else if (type == typeof(IntPtr))
            {
                return "native int";
            }
            else if (type == typeof(IntPtr*))
            {
                return "native int*";
            }
            else if (type == typeof(UIntPtr))
            {
                return "native uint";
            }
            else if (type == typeof(UIntPtr*))
            {
                return "native uint*";
            }
            else if (type == typeof(ulong))
            {
                return "uint64";
            }
            else if (type == typeof(uint))
            {
                if (type.IsPointer)
                {
                    ;
                }
                return "uint32";
            }
            else if (type == typeof(byte*))
            {
                return "uint8*";
            }
            else if (type == typeof(double*))
            {
                return "float64*";
            }
            else if (type == typeof(double))
            {
                return "float64";
            }
            else if (type == typeof(byte**))
            {
                return "uint8**";
            }
            else if (type == typeof(uint*))
            {
                return "uint32*";
            }
            else if (type.Namespace == typeof(Functions).Namespace)
            {
                return $"valuetype {type.Namespace}::{type.Name}";
            }
            else
            {
                throw new InvalidOperationException($"Missing type {type}");
            }
        }
    }
}
*/