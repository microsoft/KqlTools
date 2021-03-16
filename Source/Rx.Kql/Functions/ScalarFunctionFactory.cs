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
    using System.Linq;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reactive.Kql.Functions;
    using System.Reflection;

    public static class ScalarFunctionFactory
    {
        private static readonly IReadOnlyDictionary<string, ConstructorInfo> mapping;

        private static readonly Dictionary<string, MethodInfo> CustomFunctions;

        static ScalarFunctionFactory()
        {
            CustomFunctions = new Dictionary<string, MethodInfo>();

            var factories = typeof(ScalarFunctionFactory)
                .Assembly
                .GetTypes()
                .Where(type => typeof(ScalarFunction).IsAssignableFrom(type))
                .Select(type => new
                {
                    Type = type,
                    Name = type.GetCustomAttribute<DescriptionAttribute>(),
                    Constructor = type.GetConstructors()
                    .FirstOrDefault(c => c.GetParameters().Length == 0)
                })
                .Where(i => i.Constructor != null && i.Name != null)
                .Select(i => new
                {
                    Name = i.Name.Description,
                    i.Constructor,
                })
                .ToArray();

            mapping = factories
                .ToDictionary(i => i.Name, i => i.Constructor);
        }

        public static ScalarFunction Create(string name, List<ScalarValue> args)
        {
            MethodInfo method = null;
            if (CustomFunctions.TryGetValue(name, out method))
            {
                ScalarFunction newFunction = new CustomFunction(name, method)
                {
                    Arguments = args
                };

                return newFunction;
            }

            if (mapping.TryGetValue(name, out ConstructorInfo constructor))
            {
                ScalarFunction newFunction = (ScalarFunction)constructor.Invoke(new object[] { });
                newFunction.Name = name;
                newFunction.Arguments = args;
                return newFunction;
            }

            if (GlobalFunctions.KqlFunctions.TryGetValue(name, out var cslFun))
            {
                return new UserDefinedFunction
                {
                    Name = name,
                    Parameters = cslFun.Arguments,
                    Body = cslFun.Body,
                    Arguments = args,
                };
            }

            throw new UnknownFunctionException($"Unknown function: '{name}'");
        }

        /// <summary>
        /// Adds functions to be used inside Rx.KQL queries
        /// Functions must be static and marked with attribute <see cref="KqlScalarFunctionAttribute"/>
        /// </summary>
        /// <param name="type">The type implementing the functions</param>
        public static void AddFunctions(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .ToArray();

            foreach (var m in methods)
            {
                var attribute = m.GetCustomAttribute<KqlScalarFunctionAttribute>();
                if (attribute != null && 
                    !CustomFunctions.ContainsKey(attribute.Name))
                {
                    CustomFunctions.Add(attribute.Name, m);
                }
            }
        }
    }
}