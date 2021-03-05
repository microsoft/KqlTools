// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reflection;

    public static class BinaryExpressionFactory
    {
        private static readonly IReadOnlyDictionary<string, ConstructorInfo> mapping;

        static BinaryExpressionFactory()
        {
            var expressions = typeof(BinaryExpressionFactory)
                .Assembly
                .GetTypes()
                .Where(type => typeof(BinaryExpression).IsAssignableFrom(type))
                .Select(type => new
                {
                    Type = type,
                    Operators = type.GetCustomAttribute<OperatorAttribute>()?.Operators,
                    Constructor = type.GetConstructors()
                    .FirstOrDefault(c => c.GetParameters().Length == 0)
                })
                .Where(i => i.Constructor != null && i.Operators != null);

            var mappingConstruct = new Dictionary<string, ConstructorInfo>();
            foreach (var exp in expressions)
            {
                foreach (var op in exp.Operators)
                {
                    mappingConstruct.Add(op, exp.Constructor);
                }
            }
            mapping = mappingConstruct;
        }

        public static BinaryExpression Create(string name, ScalarValue left, ScalarValue right)
        {
            ConstructorInfo constructor;
            if (!mapping.TryGetValue(name, out constructor))
            {
                throw new UnknownFunctionException($"Unknown function: '{name}'");
            }

            var exp = (BinaryExpression)constructor.Invoke(new object[] { });
            exp.Operator = name;
            exp.Left = left;
            exp.Right = right;
            return exp;
        }

    }
}
