using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetworkTables.Core.Native
{
    enum OsType
    {
        None,
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
            return IntPtr.Size != sizeof(int);
        }

        internal static bool IsWindows()
        {
#if NETSTANDARD1_5
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            return Path.DirectorySeparatorChar == '\\';
#endif
        }

        internal static OsType GetOsType()
        {
            if (!IsWindows())
            {
                //These 3 mean we are running on a unix based system
                //Check for RIO first
                if (File.Exists("/usr/local/frc/bin/frcRunRobot.sh")) return OsType.RoboRio;
#if NETSTANDARD1_5
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (Is64BitOs()) return OsType.Linux64;
                    else return OsType.Linux32;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (Is64BitOs()) return OsType.MacOs64;
                    else return OsType.MacOs32;
                }
                else
                {
                    return OsType.None;
                }
            }
#else
                Utsname uname;
                try
                {
                    //Try to grab uname. On android this fails, so we can assume android
                    Uname.uname(out uname);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
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
#endif
            else
            {
                //Assume we are on windows otherwise
                return Is64BitOs() ? OsType.Windows64 : OsType.Windows32;
            }

        }

        internal static bool CheckOsValid(OsType type)
        {
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
                    return false;
                case OsType.RoboRio:
                    return true;
                default:
                    return false;
            }
        }
        internal static void GetLibraryName(OsType type, out string embeddedResourceLocation, out string extractLocation)
        {
            switch (type)
            {
                case OsType.Windows32:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.x86.ntcore.dll";
                    break;
                case OsType.Windows64:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.amd64.ntcore.dll";
                    break;
                case OsType.Linux32:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.x86.libntcore.so";
                    break;
                case OsType.Linux64:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.amd64.libntcore.so";
                    break;
                case OsType.MacOs32:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.x86.libntcore.dylib";
                    break;
                case OsType.MacOs64:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.amd64.libntcore.dylib";
                    break;
                case OsType.RoboRio:
                    embeddedResourceLocation = "FRC.NetworkTables.Core.NativeLibraries.roborio.libntcore.so";
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
