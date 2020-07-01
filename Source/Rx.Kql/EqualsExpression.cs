// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System;
    using System.Collections.Generic;

    [Operator("==", "!=")]
    public class EqualsExpression : BinaryExpression
    {
        public override object GetValue(IDictionary<string, object> value)
        {
            var leftVal = Left.GetValue(value);
            var rightVal = Right.GetValue(value);

            switch (leftVal)
            {
                //null checks
                case null when Operator == "==":
                    return rightVal == null;
                case null when Operator == "!=":
                    return rightVal != null;

                case string s:
                    return CheckEquals(s, Convert.ToString(rightVal));
                case ushort u:
                    return CheckEquals(u, Convert.ToUInt16(rightVal));
                case int i:
                    return CheckEquals(i, Convert.ToInt32(rightVal));
                case long l:
                    return CheckEquals(l, Convert.ToInt64(rightVal));
                case decimal d:
                    return CheckEquals(d, Convert.ToDecimal(rightVal));
                case DateTime dt:
                    return CheckEquals(dt, Convert.ToDateTime(rightVal));
                case object e when leftVal.GetType().IsEnum:
                    return CheckEquals(leftVal.ToString(), rightVal.ToString());
                default:
                    throw new ArgumentException($"Unsupported type {leftVal.GetType()}");
            }
        }

        private bool CheckEquals<T>(T a, T b) where T : IEquatable<T>
        {
            switch (Operator)
            {
                case "==":
                    return a.Equals(b);
                case "!=":
                    return !a.Equals(b);
                default:
                    throw new InvalidOperationException(Operator);
            }
        }
    }
}
