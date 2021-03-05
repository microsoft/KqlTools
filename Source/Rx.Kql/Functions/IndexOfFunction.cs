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

    [Description("indexof")]
    public class IndexOfFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public IndexOfFunction()
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
                    return source.IndexOf(lookup, StringComparison.Ordinal);

                case 3: // The starting index for the search
                    var startIndex = Convert.ToInt32(Arguments[2].GetValue(evt));
                    return source.IndexOf(lookup, startIndex, StringComparison.Ordinal);

                case 4: // The length for the search
                    var startIndexLength = Convert.ToInt32(Arguments[2].GetValue(evt));
                    var length = Convert.ToInt32(Arguments[3].GetValue(evt));
                    return source.IndexOf(lookup, startIndexLength, length, StringComparison.Ordinal);

                case 5: // The number of the occurrence in the string, if it exists
                    int i = 1;
                    int index = 0;
                    var occurrence = Convert.ToInt32(Arguments[4].GetValue(evt));

                    while (i <= occurrence && (index = source.IndexOf(lookup, index + 1, StringComparison.Ordinal)) != -1)
                    {
                        if (i == occurrence)
                        {
                            return index;
                        }

                        i++;
                    }

                    return -1;
            }

            return -1;
        }
    }
}