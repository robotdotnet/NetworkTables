using System;
using System.Runtime.InteropServices;

namespace NetworkTables.Core.Native
{
    internal interface ILibraryLoader
    {
        IntPtr LoadLibrary(string filename);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);

        void UnloadLibrary(IntPtr handle);
    }

    [ExcludeFromCodeCoverage]
    internal class WindowsLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            return LoadLibrary(filename);
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            IntPtr addr = GetProcAddress(dllHandle, name);
            if (addr == IntPtr.Zero)
            {
                //Address not found. Throw Exception
                throw new Exception($"Method not found: {name}");
            }
            return addr;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            FreeLibrary(handle);
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);

        [DllImport("kernel32")]
        private static extern bool FreeLibrary(IntPtr handle);
    }

    [ExcludeFromCodeCoverage]
    internal class AndroidLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero) return dl;
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
            return dl;
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            dlerror();
            IntPtr result = dlsym(dllHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);
    }


    [ExcludeFromCodeCoverage]
    internal class LinuxLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero) return dl;
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
            return dl;
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            dlerror();
            IntPtr result = dlsym(dllHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        [DllImport("dl")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("dl")]
        private static extern IntPtr dlerror();

        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
    }


    [ExcludeFromCodeCoverage]
    internal class MacOsLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero) return dl;
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
            return dl;
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            dlerror();
            IntPtr result = dlsym(dllHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        [DllImport("dl")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("dl")]
        private static extern IntPtr dlerror();

        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
    }

    [ExcludeFromCodeCoverage]
    internal class RoboRioLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero) return dl;
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
            return dl;
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            dlerror();
            IntPtr result = dlsym(dllHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        [DllImport("libdl-2.20.so")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl-2.20.so")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl-2.20.so")]
        private static extern IntPtr dlerror();

        [DllImport("libdl-2.20.so")]
        private static extern int dlclose(IntPtr handle);
    }
}
