// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/
#if NETFULL
namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.CommonUtilities;
    using System.Text.RegularExpressions;

    [Description("cpu_usage")]
    public class CpuUtilization : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public CpuUtilization()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            decimal cpuUtilization = (decimal) PerfCounterHelper.GetCurrentCpuCounter();

            return cpuUtilization;
        }
    }
}
#endif