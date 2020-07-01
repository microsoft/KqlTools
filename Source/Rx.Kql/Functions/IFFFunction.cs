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

    [Description("iff")]
    public class IffFunction : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            return RxKqlCommonFunctions.GetImmediateIfValue(evt, Arguments);
        }
    }
}