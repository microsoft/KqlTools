using System.Diagnostics;
using System.Runtime.Caching;
using System.Reactive.Kql;
using System;

namespace RealTimeKql
{
    public static partial class CustomScalarFunctions
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
                if (string.IsNullOrWhiteSpace(ret))
                {
                    ret = pid.ToString();
                }
            }

            // Only keep the item in cache only for 10 seconds to avoid issues with process ID reuse.  
            processNameCache.Add(pid.ToString(), ret, DateTime.Now.AddSeconds(10));
            return ret;
        }

        private static MemoryCache processNameCache = new MemoryCache("ProcessNameCache");
    }
}