// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System.Collections.Generic;
    using System.Text;

    [Operator("between")]
    public class BetweenExpression : ScalarValue
    {
        public ScalarValue Value { get; set; }
        public ScalarValue Low { get; set; }
        public ScalarValue High { get; set; }

        public BetweenExpression()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Value.GetValue(evt);
            var low = Low.GetValue(evt);
            var high = High.GetValue(evt);
            return RxKqlCommonFunctions.EvaluateBetween(value, low, high);
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder(Value.ToString());
            sb.Append(" between (");
            sb.Append(Low.ToString());
            sb.Append(" .. ");
            sb.Append(High.ToString());
            sb.Append(")");
            return sb.ToString();
        }
    }
}