// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    [Operator("=~", "!~")]
    public class EqualsTildeExpression : BinaryExpression
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftVal = Left.GetValue(evt).ToString();
            var rightVal = Right.GetValue(evt).ToString();

            switch (Operator)
            {
                case "=~":
                    return string.Compare(leftVal, rightVal, true, CultureInfo.InvariantCulture) == 0;
                case "!~":
                    return string.Compare(leftVal, rightVal, true, CultureInfo.InvariantCulture) != 0;
                default:
                    throw new InvalidOperationException(Operator);
            }
        }
    }
}
