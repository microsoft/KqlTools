// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using ComponentModel;
    using System.Collections.Generic;
    using System.Linq;

    [Description("coalesce")]
    public class CoalesceFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public CoalesceFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            object firstNotNull = null;
            foreach (ScalarValue scalar in Arguments)
            {
                var value = scalar.GetValue(evt);

                if (value is string && string.IsNullOrEmpty(value.ToString()))
                {
                    continue;
                }

                if (value != null)
                {
                    firstNotNull = value;
                    break;
                }
            }

            return firstNotNull;
        }
    }
}
