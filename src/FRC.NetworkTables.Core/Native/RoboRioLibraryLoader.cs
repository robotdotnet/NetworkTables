﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using NetworkTables.Core.NativeLibraryUtilities;

namespace NetworkTables.Core.Native
{
    [ExcludeFromCodeCoverage]
    internal class RoboRioLibraryLoader : ILibraryLoader
    {
        /// <inheritdoc/>
        public IntPtr NativeLibraryHandle { get; private set; } = IntPtr.Zero;

        /// <inheritdoc/>
        void ILibraryLoader.LoadLibrary(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("The file requested to be loaded could not be found");
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero)
            {
                NativeLibraryHandle = dl;
                return;
            };
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
        }

        /// <inheritdoc/>
        IntPtr ILibraryLoader.GetProcAddress(string name)
        {
            dlerror();
            IntPtr result = dlsym(NativeLibraryHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        /// <inheritdoc/>
        void ILibraryLoader.UnloadLibrary()
        {
            dlclose(NativeLibraryHandle);
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
