// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.Functions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql;

    [Description("iif")]
    public class IifFunction : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            return RxKqlCommonFunctions.GetImmediateIfValue(evt, Arguments);
        }
    }
}