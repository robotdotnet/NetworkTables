using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETSTANDARD1_3
using System.Runtime.InteropServices;
#endif
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal static class RuntimeDetector
    {
        enum ProperSocketsCacheState
        {
            NotCached,
            Supported,
            NotSupported
        }

        private static ProperSocketsCacheState s_socketState = ProperSocketsCacheState.NotCached;

        /// <summary>
        /// Gets if the runtime has sockets that support proper connections
        /// </summary>
        /// <returns></returns>
        public static bool GetRuntimeHasProperSockets()
        {
            if (s_socketState == ProperSocketsCacheState.NotCached)
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type == null)
                {
#if NETSTANDARD1_3
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Windows
                        s_socketState = ProperSocketsCacheState.Supported;
                        return true;
                    }
                    else
                    {
                        // Unix
                        s_socketState = ProperSocketsCacheState.NotSupported;
                        return false;
                    }
#else
                    // Full .net framework works perfectly.
                    s_socketState = ProperSocketsCacheState.Supported;
                    return true;
#endif
                }
                // For now mono does not support, so return false
                s_socketState = ProperSocketsCacheState.NotSupported;
                return false;
            }
            return s_socketState == ProperSocketsCacheState.Supported;
        }
    }
}
