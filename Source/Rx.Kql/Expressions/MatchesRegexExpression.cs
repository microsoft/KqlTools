// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System.Collections.Generic;
    using System.Text;

    [Operator("matches regex")]
    public class MatchesRegexExpression : BinaryExpression
    {
        public MatchesRegexExpression()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Left.GetValue(evt);
            var pattern = Right.GetValue(evt);
            return RxKqlCommonFunctions.EvaluateMatchesRegex(value, pattern);
        }

        public override string ToString()
        {
            return $"{Left.ToString()} matches regex {Right}";
        }
    }
}