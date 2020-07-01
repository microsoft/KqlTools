// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Attribute used to mark C# method as function available in Rx.KQL queries
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class KqlScalarFunctionAttribute : Attribute
    {
        public string Name { get; private set; }

        public KqlScalarFunctionAttribute(string name)
        {
            Name = name;
        }
    }

    public class CustomFunction : ScalarFunction
    {
        public MethodInfo Method { get; private set; }

        public CustomFunction(string name, MethodInfo method) : base()
        {
            Name = name;
            Method = method;
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            List<object> parameters = new List<object>();

            foreach (var arg in Arguments)
            {
                object value = arg.GetValue(evt);
                parameters.Add(value);
            }

            var result= Method.Invoke(null, parameters.ToArray());
            return result;
        }
    }
}
