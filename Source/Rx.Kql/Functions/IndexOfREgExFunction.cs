// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;

    [Description("indexof_regex")]
    public class IndexOfRegExFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public IndexOfRegExFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var source = Arguments[0].GetValue(evt).ToString();
            var lookup = Arguments[1].GetValue(evt).ToString();

            // Execute based on parameter count value
            switch (Arguments.Count)
            {
                case 2: // A basic search 
                    var m = Regex.Match(source, lookup);
                    return m.Success ? m.Index : -1;

                case 3: // The starting index for the search
                    return -1;

                case 4: // The length for the search
                    return -1;

                case 5: // The number of the occurrence in the string, if it exists
                    var startIndex = Convert.ToInt32(Arguments[2].GetValue(evt));
                    var length = Convert.ToInt32(Arguments[3].GetValue(evt));

                    // Retrieve the substring of the original to search
                    string stringToSearch = source.Substring(startIndex, length);

                    // Apply the Regular Expression, and return the index of the requested match.
                    var pattern = new Regex(lookup);
                    var allMatches = pattern.Matches(stringToSearch);
                    var occurrence = Convert.ToInt32(Arguments[4].GetValue(evt));
                    if (allMatches.Count == 0 || occurrence > allMatches.Count)
                    {
                        return -1;
                    }

                    // return the index of the Regular Expression match
                    return allMatches[occurrence - 1].Index;
            }

            return -1;
        }
    }
}