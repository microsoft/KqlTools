// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class UserDefinedFunction : ScalarFunction
    {
        public List<Tuple<string, string>> Parameters { get; set; }

        public ScalarValue Body { get; set; }

        public override object GetValue(IDictionary<string, object> evt)
        {
            if (Arguments.Count != Parameters.Count)
            {
                throw new ArgumentException($"Number of arguments ({Arguments.Count}) given doesn't match expected number of arguments ({Parameters.Count})");
            }
            var argValues = Arguments.Select(a => a.GetValue(evt)).ToList();
            var functionEvt = Parameters.Zip(argValues, (param, arg) => new { paramName = param.Item1, arg })
                .ToDictionary(t => t.paramName, t => t.arg);
            return Body.GetValue(functionEvt);
    }
        }
}
