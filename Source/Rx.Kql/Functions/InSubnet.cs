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

    [Description("InSubnet")]
    class InSubnet : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            // This is placeholder implementation which only
            // works on IPv4 addresses. Jomo can provide real code
            string address = Arguments[0].GetValue(evt).ToString();
            string subnet = Arguments[1].GetValue(evt).ToString();

            int ip = BitConverter.ToInt32(IPAddress.Parse(address).GetAddressBytes(), 0);
            string[] tokens = subnet.Split('/');
            int ipSubnet = BitConverter.ToInt32(IPAddress.Parse(tokens[0]).GetAddressBytes(), 0);
            int shift = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(tokens[1])));

            return ((ip & shift) == (ipSubnet & shift));
        }
    }
}