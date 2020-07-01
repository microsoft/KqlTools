// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ScalarConst : ScalarValue
    {
        public object Value { get; set; }

        public ScalarConst()
        {

        }

        public ScalarConst(string value)
        {
            Value = value;
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            return Value;
        }

        public override string ToString()
        {
            if (Value is string)
            {
                return $"\"{Value}\"";
            }
            return Value.ToString();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", Value);
        }
    }
}