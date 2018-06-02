using AdvancedDLSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FRC.NetworkTables.Interop
{
    internal class LibraryLoader
    {
        private readonly Dictionary<OsType, string> m_nativeLibraryName = new Dictionary<OsType, string>();

        /// <inheritdoc/>
        public NativeLibraryBuilder LibraryBuilder { get; }
        /// <inheritdoc/>
        public OsType OsType { get; } = GetOsType();
        /// <inheritdoc/>
        public bool UsingTempFile { get; private set; }

        /// <inheritdoc/>
        public string LibraryLocation { get; private set; }

        public LibraryLoader(NativeLibraryBuilder builder)
        {
            LibraryBuilder = builder;
        }

        /// <summary>
        /// Checks if the current system is a roboRIO
        /// </summary>
        /// <returns>True if running on a roboRIO</returns>
        public static bool CheckIsRoboRio()
        {
            return File.Exists("/usr/local/frc/bin/frcRunRobot.sh");
        }

        /// <summary>
        /// Add a file location to be used when automatically searching for a library to load
        /// </summary>
        /// <param name="osType">The OsType to associate with the file</param>
        /// <param name="libraryName">The file to load on that OS</param>
        public void AddLibraryLocation(OsType osType, string libraryName)
        {
            m_nativeLibraryName.Add(osType, libraryName);
        }

        /// <summary>
        /// Loads a native library using the specified loader and file
        /// </summary>
        /// <typeparam name="T">The type containing the native resource, if it is embedded.</typeparam>
        /// <param name="loader">The LibraryLoader to use</param>
        /// <param name="location">The file location. Can be either an embedded resource, or a direct file location</param>
        /// <param name="directLoad">True to load the file directly from disk, otherwise false to extract from embedded</param>
        /// <param name="extractLocation">The location to extract to if the file is embedded. On null, it extracts to a temp file</param>
        public O LoadNativeLibrary<T, O>(string location, bool directLoad = false, string extractLocation = null) where O : class
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location), "Library location cannot be null");

            // Set to use temp file if extractLocation is null
            if (string.IsNullOrWhiteSpace(extractLocation) && !directLoad)
            {
                extractLocation = Path.GetTempFileName();
                UsingTempFile = true;
            }

            // RoboRIO or Direct Load
            if (directLoad)
            {
                LibraryLocation = location;
                return LibraryBuilder.ActivateInterface<O>(location);
            }
            else
            // If we are loading from extraction, extract then load
            {
                ExtractNativeLibrary<T>(location, extractLocation);
                LibraryLocation = extractLocation;
                return LibraryBuilder.ActivateInterface<O>(extractLocation);
            }
        }

        /// <summary>
        /// Loads a native library, using locations added using <see cref="AddLibraryLocation"/>
        /// </summary>
        /// <typeparam name="T">The type containing the native resource, if it is embedded.</typeparam>
        /// <param name="directLoad">True to load the file directly from disk, otherwise false to extract from embedded</param>
        /// <param name="extractLocation">The location to extract to if the file is embedded. On null, it extracts to a temp file</param>
        public O LoadNativeLibrary<T, O>(bool directLoad = false, string extractLocation = null) where O : class
        {
            OsType osType = OsType;

            if (osType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(osType) && !directLoad)
                throw new InvalidOperationException("OS Type not contained in dictionary");

            return LoadNativeLibrary<T, O>(m_nativeLibraryName[osType], directLoad, extractLocation);
        }

        /// <summary>
        /// Loads a native library with a reflected assembly holding the native libraries
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to reflect into and load from</param>
        /// <param name="localLoadOnRio">True to force a local load on the RoboRIO</param>
        public O LoadNativeLibraryFromReflectedAssembly<O>(string assemblyName, bool localLoadOnRio = true) where O : class
        {
            if (localLoadOnRio && CheckIsRoboRio())
            {
                var location = m_nativeLibraryName[OsType.roboRIO];
                LibraryLocation = location;
                return LibraryBuilder.ActivateInterface<O>(location);
            }

            AssemblyName name = new AssemblyName(assemblyName);
            Assembly asm;
            try
            {
                asm = Assembly.Load(name);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to load desktop libraries. Please ensure that the {assemblyName} is installed and referenced by your project", e);
            }

            if (OsType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(OsType))
                throw new InvalidOperationException("OS Type not contained in dictionary");

            string extractLocation = Path.GetTempFileName();
            UsingTempFile = true;

            ExtractNativeLibrary(m_nativeLibraryName[OsType], extractLocation, asm);
            
            LibraryLocation = extractLocation;
            return LibraryBuilder.ActivateInterface<O>(extractLocation);
        }

        private void ExtractNativeLibrary(string resourceLocation, string extractLocation, Assembly asm)
        {
            byte[] bytes;
            //Load our resource file into memory
            using (Stream s = asm.GetManifestResourceStream(resourceLocation))
            {
                if (s == null || s.Length == 0)
                    throw new InvalidOperationException("File to extract cannot be null or empty");
                bytes = new byte[(int)s.Length];
                s.Read(bytes, 0, (int)s.Length);
            }
            File.WriteAllBytes(extractLocation, bytes);
            GC.Collect();
        }

        private void ExtractNativeLibrary<T>(string resourceLocation, string extractLocation)
        {
            ExtractNativeLibrary(resourceLocation, extractLocation, typeof(T).Assembly);
        }

        private static bool Is64BitOs()
        {
            return IntPtr.Size != sizeof(int);
        }

        private static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// Gets the OS Type of the current running system.
        /// </summary>
        /// <returns></returns>
        public static OsType GetOsType()
        {
            if (IsWindows())
            {
                return Is64BitOs() ? OsType.Windows64 : OsType.Windows32;
            }
            else
            {
                if (CheckIsRoboRio())
                {
                    return OsType.roboRIO;
                }
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
        }
    }
}
