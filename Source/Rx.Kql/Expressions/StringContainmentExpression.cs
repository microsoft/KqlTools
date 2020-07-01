// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System;
    using System.Collections.Generic;

    [Operator("contains", "!contains", "has", "!has", "contains_cs", "!contains_cs",
        "startswith", "!startswith", "startswith_cs", "!startswith_cs", "endswith", 
        "!endswith", "endswith_cs", "!endswith_cs")]
    public class StringContainmentExpression : BinaryExpression
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftVal = Left.GetValue(evt).ToString();
            var rightVal = Right.GetValue(evt).ToString();

            switch (Operator)
            {
                case "contains":
                case "has":
                    return leftVal.ToLowerInvariant().Contains(rightVal.ToLowerInvariant());

                case "!contains":
                case "!has":
                    return !leftVal.ToLowerInvariant().Contains(rightVal.ToLowerInvariant());

                case "!contains_cs":
                    return !leftVal.Contains(rightVal);

                case "contains_cs":
                    return leftVal.Contains(rightVal);

                case "startswith":
                    return leftVal.ToLowerInvariant().StartsWith(rightVal.ToLowerInvariant());

                case "!startswith":
                    return !leftVal.ToLowerInvariant().StartsWith(rightVal.ToLowerInvariant());

                case "startswith_cs":
                    return leftVal.StartsWith(rightVal);

                case "!startswith_cs":
                    return !leftVal.StartsWith(rightVal);

                case "endswith":
                    return leftVal.ToLowerInvariant().EndsWith(rightVal.ToLowerInvariant());

                case "!endswith":
                    return !leftVal.ToLowerInvariant().EndsWith(rightVal.ToLowerInvariant());

                case "endswith_cs":
                    return leftVal.EndsWith(rightVal);

                case "!endswith_cs":
                    return !leftVal.EndsWith(rightVal);

                default:
                    throw new InvalidOperationException(Operator);
            }
        }
    }
}
