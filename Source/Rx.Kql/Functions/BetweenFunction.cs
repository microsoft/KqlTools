// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql;

    // Not an official Kusto function
    [Description("Between")]
    public class BetweenFunction : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);
            var low = Arguments[1].GetValue(evt);
            var high = Arguments[2].GetValue(evt);
            return RxKqlCommonFunctions.EvaluateBetween(value, low, high);
        }
    }
}
