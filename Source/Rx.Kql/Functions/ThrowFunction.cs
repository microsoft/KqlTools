// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.Reactive.Kql.ExceptionTypes;
    using ComponentModel;

    [Description("Throw")]
    class ThrowFunction : ScalarFunction
    {
        private int throwCounter = 0;

        public override object GetValue(IDictionary<string, object> evt)
        {
            throwCounter++;

            int threshold = 0;
            if (Arguments.Count > 0)
            {
                int.TryParse(Arguments[0].GetValue(evt).ToString(), out threshold);
            }

            if (throwCounter >= threshold)
            {
                throw new FunctionThrowException("Exception from Throw()");
            }

            return true;
        }
    }
}
