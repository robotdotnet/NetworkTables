using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetworkTables.Core.Native
{
    internal class NativeDelegateAttribute : Attribute
    {
        public string NativeName { get; }

        public NativeDelegateAttribute()
        {
            NativeName = null;
        }

        public NativeDelegateAttribute(string nativeName)
        {
            NativeName = nativeName;
        }
    }
    internal static class DelegateInitializer
    {
        public static void SetupDelegates(Type setupType, IntPtr library, ILibraryLoader loader)
        {
            var info = setupType.GetTypeInfo();
#if NETSTANDARD
            MethodInfo getDelegateForFunctionPointer =
                typeof(Marshal).GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "GetDelegateForFunctionPointer" && m.IsGenericMethod);
#endif
            foreach (var field in info.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = (NativeDelegateAttribute)field.GetCustomAttribute(typeof(NativeDelegateAttribute));
                if (attribute == null) continue;
                string nativeName = attribute.NativeName ?? field.Name;
#if NETSTANDARD
                MethodInfo delegateGetter = getDelegateForFunctionPointer.MakeGenericMethod(field.FieldType);
                object setVal = delegateGetter.Invoke(null, new object[] { loader.GetProcAddress(library, nativeName) });
#else
                object setVal = Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(library, nativeName),
                    field.FieldType);
#endif
                field.SetValue(null, setVal);
            }
        }
    }
}
