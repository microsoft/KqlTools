// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("ipv4_fromnumber")]
    public class Ipv4_FromNumberFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public Ipv4_FromNumberFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var argValue = Arguments[0].GetValue(evt);
            if (!RxKqlCommonFunctions.TryConvert<long>(argValue, out var longIp))
            {
                return string.Empty;
            }
            return RxKqlCommonFunctions.LongToIpAddress(longIp);
        }
    }
}