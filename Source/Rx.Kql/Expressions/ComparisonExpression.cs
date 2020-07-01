// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Kql.ExceptionTypes;

    [Operator("<", ">", "<=", ">=")]
    public class ComparisonExpression : BinaryExpression
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftVal = Left.GetValue(evt);
            var rightVal = Right.GetValue(evt);
            
            if (leftVal is ushort)
            {
                return Compare((ushort)leftVal, Convert.ToUInt16(rightVal));
            }

            if (leftVal is int)
            {
                return Compare((int)leftVal, Convert.ToInt32(rightVal));
            }

            if (leftVal is long)
            {
                return Compare((long)leftVal, Convert.ToInt64(rightVal));
            }

            if (leftVal is decimal)
            {
                return Compare((decimal)leftVal, Convert.ToDecimal(rightVal));
            }

            if (leftVal is DateTime)
            {
                return Compare((DateTime)leftVal, Convert.ToDateTime(rightVal));
            }

            if (leftVal is DateTimeOffset)
            {
                DateTimeOffset dto = (DateTimeOffset)leftVal;
                return Compare(dto.DateTime, Convert.ToDateTime(rightVal));
            }

            throw new EvaluationTypeMismatchException($"Cannot perform operation {Operator} on types {leftVal.GetType()} and {rightVal.GetType()}");
        }

        private bool Compare<T>(T a, T b) where T : IComparable
        {
            switch (Operator)
            {
                case "<":
                    return a.CompareTo(b) < 0;
                case ">":
                    return a.CompareTo(b) > 0;
                case "<=":
                    return a.CompareTo(b) <= 0;
                case ">=":
                    return a.CompareTo(b) >= 0;
                case "==":
                    return a.CompareTo(b) == 0;
                case "!=":
                    return a.CompareTo(b) != 0;
                default:
                    throw new InvalidOperationException(Operator);
            }
        }
    }
}
