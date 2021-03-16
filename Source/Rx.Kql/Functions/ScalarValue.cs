// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;

    public abstract class ScalarValue
    {
        public abstract object GetValue(IDictionary<string, object> evt);
    }

    public class ScalarValueList : ScalarValue
    {
        public List<ScalarValue> List { get; set; } = new List<ScalarValue>();

        public override object GetValue(IDictionary<string, object> evt)
        {
            throw new NotImplementedException();
        }
    }
}