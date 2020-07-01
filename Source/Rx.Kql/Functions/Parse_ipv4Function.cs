// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("parse_ipv4")]
    public class Parse_ipv4Function : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public Parse_ipv4Function()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var arg = Arguments[0].GetValue(evt).ToString();
            return RxKqlCommonFunctions.IpAddressToLong(arg);
        }
    }
}