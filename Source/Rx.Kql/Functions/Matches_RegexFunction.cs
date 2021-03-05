// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.ExceptionTypes;

    [Description("matches regex")]
    public class MatchesRegex : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public MatchesRegex()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string input = Arguments[0].GetValue(evt).ToString();
            string pattern = Arguments[1].GetValue(evt).ToString();

            // Evaluate for matches and return if any matches exist in input
            MatchCollection matches = Regex.Matches(input, pattern);

            return matches.Count > 0;
        }
    }
}