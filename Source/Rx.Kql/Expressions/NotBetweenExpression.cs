// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    [Description("!between")]
    public class NotBetweenExpression : ScalarValue
    {
        public ScalarValue Value { get; set; }
        public ScalarValue Low { get; set; }
        public ScalarValue High { get; set; }

        public NotBetweenExpression()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var v = Value.GetValue(evt);
            var low = Low.GetValue(evt);
            var high = High.GetValue(evt);

            // Negate the return from this Expression, as it's NOT between.
            return !RxKqlCommonFunctions.EvaluateBetween(v, low, high);
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder(Value.ToString());
            sb.Append(" !between (");
            sb.Append(Low.ToString());
            sb.Append(" .. ");
            sb.Append(High.ToString());
            sb.Append(")");
            return sb.ToString();
        }
    }
}