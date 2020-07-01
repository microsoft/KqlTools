// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Text;

    [Description("strcat")]
    public class StrcatFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public StrcatFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var builder = new StringBuilder();
            foreach (var arg in Arguments)
            {
                builder.Append(arg.GetValue(evt)?.ToString() ?? string.Empty);
            }

            return builder.ToString();
        }
    }
}