// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using System.Text;

    /// <summary>
    ///     Record of sampling all the counters of particular instance
    ///     E.g. all the PMU counters on one processor
    /// </summary>
    public class CounterSample
    {
        public DateTime Timestamp { get; set; }

        public string Machine { get; set; }

        public string Instance { get; set; }

        public dynamic Counters { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Timestamp.ToLongTimeString());
            sb.Append(": ");
            sb.Append(Machine);
            sb.Append("[");
            sb.Append(Instance);
            sb.Append("]");
            sb.Append(Counters);

            return sb.ToString();
        }
    }
}