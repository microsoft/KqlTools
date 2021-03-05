// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.CustomTypes
{
    using System.Collections.Generic;

    [Serializable]
    public class RxKqlScalarValue : ScalarValue
    {
        public string Left { get; set; }

        public string Operator { get; set; } = "=";

        public ScalarValue Right { get; set; }

        public override object GetValue(IDictionary<string, object> evt)
        {
            throw new NotImplementedException();
        }
    }
}