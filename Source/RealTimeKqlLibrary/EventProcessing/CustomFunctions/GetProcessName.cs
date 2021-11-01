using System;
using System.Diagnostics;
using System.Reactive.Kql;
using System.Runtime.Caching;

namespace RealTimeKqlLibrary
{
    public static partial class RealTimeCustomScalarFunctions
    {
        [KqlScalarFunction("getprocessname")]
        public static string GetProcessName(uint pid)
        {
            if (processNameCache.Contains(pid.ToString()))
            {
                return processNameCache.Get(pid.ToString()).ToString();
            }

            string ret = string.Empty;
            Process proc = null;
            try { proc = Process.GetProcessById((int)pid); }
            catch (Exception) { }

            try
            {
                if (proc != null)
                {
                    ret = proc.ProcessName;
                }
            }
            catch
            {
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(ret))
                {
                    // If we got an actual name for the process, add it to the cache
                    // Only keep the item in cache only for 10 seconds to avoid issues with process ID reuse.  
                    processNameCache.Add(pid.ToString(), ret, DateTime.Now.AddSeconds(10));
                }
            }
            
            return ret;
        }

        private static MemoryCache processNameCache = new MemoryCache("ProcessNameCache");
    }
}