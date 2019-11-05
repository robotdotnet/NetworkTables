using FRC.NativeLibraryUtilities;
using System;
using System.IO;

namespace FRC.NetworkTables.Interop
{
  public static class NtCoreHelper
  {
    public static bool LoadOnStaticInit { get; set;} = true;

    public static INtCore? Load()
    {
            var nativeLoader = new NativeLibraryLoader();

            string[] commandArgs = Environment.GetCommandLineArgs();
            foreach (var commandArg in commandArgs)
            {
                //search for a line with the prefix "-ntcore:"
                if (commandArg.ToLower().Contains("-ntcore:"))
                {
                    //Split line to get the library.
                    int splitLoc = commandArg.IndexOf(':');
                    string file = commandArg.Substring(splitLoc + 1);

                    //If the file exists, just return it so dlopen can load it.
                    if (File.Exists(file))
                    {
                        nativeLoader.LoadNativeLibrary<INtCore>(file, true);
                        return nativeLoader.LoadNativeInterface<INtCore>();
                    }
                }
            }

            if (nativeLoader.TryLoadNativeLibraryPath("ntcorejni"))
            {
                return nativeLoader.LoadNativeInterface<INtCore>();
            }

            const string resourceRoot = "FRC.NetworkTables.Core.DesktopLibraries.libraries.";

            nativeLoader.AddLibraryLocation(OsType.Windows32,
                resourceRoot + "windows.x86.ntcorejni.dll");
            nativeLoader.AddLibraryLocation(OsType.Windows64,
                resourceRoot + "windows.x86_64.ntcorejni.dll");
            nativeLoader.AddLibraryLocation(OsType.Linux64,
                resourceRoot + "linux.x86_64.libntcorejni.so");
            nativeLoader.AddLibraryLocation(OsType.MacOs64,
                resourceRoot + "osx.x86_64.libntcorejni.dylib");
            nativeLoader.AddLibraryLocation(OsType.roboRIO, "ntcore");

            nativeLoader.LoadNativeLibraryFromReflectedAssembly("FRC.NetworkTables.Core.DesktopLibraries");
            return nativeLoader.LoadNativeInterface<INtCore>();
        }


  }
}
