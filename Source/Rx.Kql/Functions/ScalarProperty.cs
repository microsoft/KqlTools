// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ScalarProperty : ScalarValue
    {
        public string Value { get; }

        public ScalarProperty(string value)
        {
            Value = value;
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            return evt.TryGetValue(Value, out object retVal) ? retVal ?? string.Empty : string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Value);
        }
    }
}