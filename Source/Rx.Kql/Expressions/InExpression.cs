// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

using System.ComponentModel;

namespace System.Reactive.Kql.Expressions
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    [Description("in")]
    public class InExpression : ScalarValue
    {
        public bool CaseInsensitive { get; set; }

        public ScalarValue Left { get; set; }
        public List<ScalarValue> Right { get; set; }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftVal = Left.GetValue(evt);
            var rightVals = Right.Select(s => s.GetValue(evt));
            return rightVals.Where(e => EqualsMaybeCS(leftVal, e)).Any();
        }

        private bool EqualsMaybeCS(object a, object b)
        {
            if (CaseInsensitive)
            {
                return string.Compare(a.ToString(), b.ToString(), true, CultureInfo.InvariantCulture) == 0;
            }
            return RxKqlCommonFunctions.TryConvert(b, a.GetType(), out var bConverted) && a.Equals(bConverted);
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder(Left.ToString());
            sb.Append($" in{(CaseInsensitive ? "~" : string.Empty)} (");
            foreach (var v in Right)
            {
                sb.Append(prefix);
                prefix = ", ";
                sb.Append($"\"{v}\"");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}