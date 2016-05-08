using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetworkTables.Native
{
    enum OsType
    {
        Windows32,
        Windows64,
        Linux32,
        Linux64,
        MacOs32,
        MacOs64,
        Armv6HardFloat, //Raspberry Pi 1. Has a library, but probably won't update.
        Armv7HardFloat,
        Android,
        RoboRio//RoboRIO is Armv7 Soft Float
    }

    [ExcludeFromCodeCoverage]
    internal static class LoaderUtilities
    {
        internal static bool Is64BitOs()
         {
             return Marshal.SizeOf(typeof(IntPtr)) == 8;
         }
 
         internal static bool IsWindows()
         {
             return Path.DirectorySeparatorChar == '\\';
         }

internal static OsType GetOsType()
        {
            if (!IsWindows())
            {
                //These 3 mean we are running on a unix based system
                //Check for RIO first
                if (File.Exists("/usr/local/frc/bin/frcRunRobot.sh")) return OsType.RoboRio;

                Utsname uname;
                try
                {
                    //Try to grab uname. On android this fails, so we can assume android
                    Uname.uname(out uname);
                }
                catch
                {
                    return OsType.Android;
                }


                Console.WriteLine(uname.ToString());

                bool mac = uname.sysname == "Darwin";
                bool armv6 = uname.machine.ToLower().Contains("armv6");
                bool armv7 = uname.machine.ToLower().Contains("armv7");

                if (armv6) return OsType.Armv6HardFloat;
                if (armv7) return OsType.Armv7HardFloat;

                //Check for Bitness
                if (Is64BitOs())
                {
                    //We are 64 bit.
                    if (mac) return OsType.MacOs64;
                    return OsType.Linux64;
                }
                else
                {
                    //We are 64 32 bit process.
                    if (mac) return OsType.MacOs32;
                    return OsType.Linux32;
                }


            }
            else
            {
                //Assume we are on windows otherwise
                return Is64BitOs() ? OsType.Windows64 : OsType.Windows32;
            }
        }

        internal static bool CheckOsValid(OsType type)
        {
#if ARMSTANDALONE
            switch (type)
            {
                case OsType.Windows32:
                    return false;
                case OsType.Windows64:
                    return false;
                case OsType.Linux32:
                    return false;
                case OsType.Linux64:
                    return false;
                case OsType.MacOs32:
                    return false;
                case OsType.MacOs64:
                    return false;
                case OsType.Armv6HardFloat:
                    Console.WriteLine("Raspberry Pi 1 does work, however the library will not be often updated and will probably be out of date.");
                    return true;
                case OsType.Armv7HardFloat:
                    return true;
                case OsType.Android:
                    return true; //The ArmV7 binary is not working currently for android. Need to get that working.
                case OsType.RoboRio:
                    return true;
                default:
                    return false;
            }
#else
            switch (type)
            {
                case OsType.Windows32:
                    return true;
                case OsType.Windows64:
                    return true;
                case OsType.Linux32:
                    return true;
                case OsType.Linux64:
                    return true;
                case OsType.MacOs32:
                    return true;
                case OsType.MacOs64:
                    return true;
                case OsType.Armv6HardFloat:
                    return false;
                case OsType.Armv7HardFloat:
                    return false;
                case OsType.Android:
                    return false; //The ArmV7 binary is not working currently for android. Need to get that working.
                case OsType.RoboRio:
                    return true;
                default:
                    return false;
            }
#endif
        }
        internal static void GetLibraryName(OsType type, out string embeddedResourceLocation, out string extractLocation)
        {
            switch (type)
            {
                case OsType.Windows32:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.x86.ntcore.dll";
                    //extractLocation = "ntcore.dll";
                    break;
                case OsType.Windows64:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.amd64.ntcore.dll";
                    //extractLocation = "ntcore.dll";
                    break;
                case OsType.Linux32:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.x86.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                case OsType.Linux64:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.amd64.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                case OsType.MacOs32:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.x86.libntcore.dylib";
                    //extractLocation = "libntcore.dylib";
                    break;
                case OsType.MacOs64:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.amd64.libntcore.dylib";
                    //extractLocation = "libntcore.dylib";
                    break;
                case OsType.RoboRio:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.roborio.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                case OsType.Armv6HardFloat:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.armv6.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                // Android is only Arm Android. Don't currently have a way to detect otherwise.
                case OsType.Android:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.android.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                case OsType.Armv7HardFloat:
                    embeddedResourceLocation = "NetworkTables.NativeLibraries.armv7.libntcore.so";
                    //extractLocation = "libntcore.so";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            extractLocation = Path.GetTempFileName();
        }


        internal static bool ExtractLibrary(string embeddedResourceLocation, ref string extractLocation)
        {
            byte[] bytes;
            //Load our resource file into memory
            using (Stream s = typeof(LoaderUtilities).GetTypeInfo().Assembly.GetManifestResourceStream(embeddedResourceLocation))
            {
                if (s == null || s.Length == 0)
                    return false;
                bytes = new byte[(int)s.Length];
                s.Read(bytes, 0, (int)s.Length);
            }
            File.WriteAllBytes(extractLocation, bytes);
            /*
            bool isFileSame = true;
            try
            {
                //If file exists
                if (File.Exists(extractLocation))
                {
                    //Load existing file into memory
                    byte[] existingFile = File.ReadAllBytes(extractLocation);
                    //If files are different length they are different,
                    //and we can automatically assume they are different.
                    if (existingFile.Length != bytes.Length)
                    {
                        isFileSame = false;
                    }
                    else
                    {
                        //Otherwise directly compare the files
                        //I first tried hashing, but that took 1.5-2.0 seconds,
                        //wheras this took 0.3 seconds.
                        for (int i = 0; i < existingFile.Length; i++)
                        {
                            if (bytes[i] != existingFile[i])
                            {
                                isFileSame = false;
                            }
                        }
                    }
                }
                else
                {
                    isFileSame = false;
                }

                //If file is different write the new file
                if (!isFileSame)
                {
                    if (File.Exists(extractLocation))
                        File.Delete(extractLocation);
                    File.WriteAllBytes(extractLocation, bytes);
                }

            }
            //If IO exception, means something else is using ntcore. Write to unique file.
            catch (IOException)
            {
                extractLocation = Path.GetTempFileName();
                File.WriteAllBytes(extractLocation, bytes);
            }*/
            //Force a garbage collection, since we just wasted about 12 MB of RAM.
            GC.Collect();

            return true;

        }

        internal static IntPtr LoadLibrary(string dllLoc, OsType type, out ILibraryLoader loader)
        {
            switch (type)
            {
                case OsType.Windows32:
                case OsType.Windows64:
                    loader = new WindowsLibraryLoader();
                    return loader.LoadLibrary(dllLoc);
                case OsType.Linux32:
                case OsType.Linux64:
                case OsType.Armv6HardFloat:
                case OsType.Armv7HardFloat:
                    loader = new LinuxLibraryLoader();
                    return loader.LoadLibrary(dllLoc);
                case OsType.MacOs32:
                case OsType.MacOs64:
                    loader = new MacOsLibraryLoader();
                    return loader.LoadLibrary(dllLoc);
                case OsType.RoboRio:
                    loader = new RoboRioLibraryLoader();
                    return loader.LoadLibrary(dllLoc);
                case OsType.Android:
                    loader = new AndroidLibraryLoader();
                    return loader.LoadLibrary(dllLoc);
                default:
                    loader = null;
                    return IntPtr.Zero;
            }
        }
    }
}
