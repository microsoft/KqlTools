// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.ExceptionTypes;

    [Description("ago")]
    public class AgoFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public AgoFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            DateTime now = DateTime.UtcNow;

            var ago = Arguments[0].GetValue(evt);

            if (!(ago is TimeSpan))
            {
                throw new EvaluationTypeMismatchException($"{ago} is not type TimeSpan");
            }

            return now.Add(-((TimeSpan)ago));
        }
    }
}