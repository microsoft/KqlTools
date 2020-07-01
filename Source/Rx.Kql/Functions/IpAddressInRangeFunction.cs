// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;

    [Description("IpAddressInRange")]
    class IpAddressInRangeFunction : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            // This is placeholder implementation which only
            // works on IPv4 addresses. Jomo can provide real code
            string address = Arguments[0].GetValue(evt).ToString();
            string min = Arguments[1].GetValue(evt).ToString();
            string max = Arguments[2].GetValue(evt).ToString();

            long ipStart = BitConverter.ToInt32(IPAddress.Parse(min).GetAddressBytes().Reverse().ToArray(), 0);
            long ipEnd = BitConverter.ToInt32(IPAddress.Parse(max).GetAddressBytes().Reverse().ToArray(), 0);
            long ip = BitConverter.ToInt32(IPAddress.Parse(address).GetAddressBytes().Reverse().ToArray(), 0);
            return ip >= ipStart && ip <= ipEnd;
        }
    }
}
