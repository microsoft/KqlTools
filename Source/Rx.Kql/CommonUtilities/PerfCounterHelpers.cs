// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/
#if NETFULL
namespace System.Reactive.Kql.CommonUtilities
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class PerfCounterHelper
    {
        private static readonly PerformanceCounter CpuCounter;

        private static readonly PerformanceCounter MemCounter;

        static PerfCounterHelper()
        {
            try
            {
                CpuCounter = new PerformanceCounter("Processor",
                    "% Processor Time", "_Total");

                MemCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static float GetCurrentCpuCounter()
        {
            return CpuCounter?.NextValue() ?? 0;
        }

        public static float GetCurrentMemoryCounter()
        {
            return MemCounter?.NextValue() ?? 0;
        }

        public static double GetNetworkUtilization(string specialInstanceName = null)
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
                List<string> instancenames = category.GetInstanceNames().ToList();

                var instanceName = !string.IsNullOrWhiteSpace(specialInstanceName) &&
                                   instancenames.Contains(specialInstanceName)
                    ? specialInstanceName
                    : instancenames[0];

                const int NumberOfIterations = 10;

                var bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", instanceName);
                float bandwidth = bandwidthCounter.NextValue();

                var dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName);

                var dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName);

                float sendSum = 0;
                float receiveSum = 0;

                for (int index = 0; index < NumberOfIterations; index++)
                {
                    sendSum += dataSentCounter.NextValue();
                    receiveSum += dataReceivedCounter.NextValue();
                }

                float dataSent = sendSum;
                float dataReceived = receiveSum;

                return (8 * (dataSent + dataReceived)) / (bandwidth * NumberOfIterations) * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}

#endif