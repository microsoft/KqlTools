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
    using System.Reflection;

    /// <summary>
    ///     Base class for functions like count(), dcount(x), min(x), max(x), etc.
    /// </summary>
    public abstract class AggregationFunction : ScalarFunction
    {
        public abstract object DefaultValue { get; }

        protected abstract object AddScalar(object inputValue, object outputValue);

        public override object GetValue(IDictionary<string, object> evt)
        {
            throw new NotImplementedException();
        }

        public AggregationFunction(IEnumerator<ScalarValue> args)
        {
            args.Reset();

            while (args.MoveNext())
            {
                if (Arguments == null)
                {
                    Arguments = new List<ScalarValue>();
                }

                Arguments.Add(args.Current);
            }

            args.Reset();
        }

        public void AddEvent(
            IDictionary<string, object> inputEvent,
            ScalarValue inputExpression,
            IDictionary<string, object> outputEvent,
            string outputPropertyName)
        {
            object inputValue = null;

            if (inputExpression != null)
            {
                inputValue = inputExpression.GetValue(inputEvent);
            }

            object aggregatedSoFar = outputEvent[outputPropertyName];
            object newValue = AddScalar(inputValue, aggregatedSoFar);
            outputEvent[outputPropertyName] = newValue;
        }
    }

    public static class AggregationFunctionFactory // Pattern copied from ScalarFunctionFactory
    {
        private static readonly IReadOnlyDictionary<string, ConstructorInfo> mapping;

        static AggregationFunctionFactory()
        {
            var factories = typeof(AggregationFunctionFactory)
                .Assembly
                .GetTypes()
                .Where(type => typeof(AggregationFunction).IsAssignableFrom(type))
                .Select(type => new
                {
                    Type = type,
                    Name = type.GetCustomAttribute<DescriptionAttribute>(),
                    Constructor = type.GetConstructors()
                        .FirstOrDefault(c => c.GetParameters().Length == 1 &&
                        c.GetParameters()[0].ParameterType == typeof(IEnumerator<ScalarValue>))
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

        public static AggregationFunction Create(string name, List<ScalarValue> args)
        {
            if (mapping.TryGetValue(name, out ConstructorInfo constructor))
            {
                IEnumerator<ScalarValue> enumerator = args.GetEnumerator();
                AggregationFunction newFunction = (AggregationFunction)constructor.Invoke(new object[] { enumerator });
                newFunction.Name = name;
                return newFunction;
            }

            throw new UnknownFunctionException($"Unknown function: '{name}'");
        }
    }

    [Description("makeset")]
    public class MakeSetObsoleteAggregation : AggregationFunction
    {
        private readonly int maxListSize = 128;

        public MakeSetObsoleteAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
            // MakeList can contain two arguments. The second argument is the MaxListSize
            if (Arguments.Count == 2)
            {
                if (Arguments[1] is ScalarConst item &&
                    int.TryParse(item.Value.ToString(), out int listSize))
                {
                    if (listSize > 1048576)
                    {
                        listSize = 1048576;
                    }

                    maxListSize = listSize;
                }
            }
        }

        public override object DefaultValue
        {
            get
            {
                return new List<string>();
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            if (((List<string>)outputValue).Count < maxListSize &&
                !((List<string>)outputValue).Contains((string)inputValue))
            {
                ((List<string>)outputValue).Add((string)inputValue);
            }
            return outputValue;
        }
    }

    [Description("make_set")]
    public class MakeSetAggregation : AggregationFunction
    {
        private readonly int maxListSize = 1048576;

        public MakeSetAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
            // MakeList can contain two arguments. The second argument is the MaxListSize
            if (Arguments.Count == 2)
            {
                if (Arguments[1] is ScalarConst item &&
                    int.TryParse(item.Value.ToString(), out int listSize))
                {
                    if (listSize > 1048576)
                    {
                        listSize = 1048576;
                    }

                    maxListSize = listSize;
                }
            }
        }

        public override object DefaultValue
        {
            get
            {
                return new List<string>();
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            if (((List<string>)outputValue).Count < maxListSize &&
                !((List<string>)outputValue).Contains((string)inputValue))
            {
                ((List<string>)outputValue).Add((string)inputValue);
            }
            return outputValue;
        }
    }

    [Description("makelist")]
    public class MakeListObsoleteAggregation : AggregationFunction
    {
        private readonly int maxListSize = 128;

        public MakeListObsoleteAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
            // MakeList can contain two arguments. The second argument is the MaxListSize
            if (Arguments.Count == 2)
            {
                if (Arguments[1] is ScalarConst item &&
                    int.TryParse(item.Value.ToString(), out int listSize))
                {
                    if (listSize > 1048576)
                    {
                        listSize = 1048576;
                    }

                    maxListSize = listSize;
                }
            }
        }

        public override object DefaultValue
        {
            get
            {
                return new List<string>();
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            if (((List<string>)outputValue).Count < maxListSize)
            {
                ((List<string>)outputValue).Add((string)inputValue);
            }
            return outputValue;
        }
    }

    [Description("make_list")]
    public class MakeListAggregation : AggregationFunction
    {
        private readonly int maxListSize = 1048576;

        public MakeListAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
            // MakeList can contain two arguments. The second argument is the MaxListSize
            if (Arguments.Count == 2)
            {
                if (Arguments[1] is ScalarConst item &&
                    int.TryParse(item.Value.ToString(), out int listSize))
                {
                    if (listSize > 1048576)
                    {
                        listSize = 1048576;
                    }

                    maxListSize = listSize;
                }
            }
        }

        public override object DefaultValue
        {
            get
            {
                return new List<string>();
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            if (((List<string>)outputValue).Count < maxListSize)
            {
                ((List<string>)outputValue).Add((string)inputValue);
            }
            return outputValue;
        }
    }

    [Description("count")]
    public class CountAggregation : AggregationFunction
    {
        public CountAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
        }

        public override object DefaultValue
        {
            get
            {
                return (long)0;
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            return (long)outputValue + 1;
        }
    }

    [Description("sum")]
    public class SumAggregation : AggregationFunction
    {
        public SumAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
        }

        public override object DefaultValue
        {
            get
            {
                return null;
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            long increment;
            if (inputValue is int)
            {
                increment = (int)inputValue;
            }
            else if (inputValue is uint)
            {
                increment = Convert.ToInt64(inputValue);
            }
            else
            {
                increment = (long)inputValue;
            }

            if (outputValue == null)
            {
                return increment;
            }

            return (long)outputValue + increment;
        }
    }

    [Description("min")]
    public class MinAggregation : AggregationFunction
    {
        public MinAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
        }

        public override object DefaultValue
        {
            get
            {
                return null;
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            long input;
            if (inputValue.GetType() == typeof(int))
            {
                input = (long)(int)inputValue;
            }
            else
            {
                input = (long)inputValue;
            }

            if (outputValue == null)
            {
                return input;
            }

            return Math.Min((long)outputValue, input);
        }
    }

    [Description("max")]
    public class MaxAggregation : AggregationFunction
    {
        public MaxAggregation(IEnumerator<ScalarValue> args) : base(args)
        {
        }

        public override object DefaultValue
        {
            get
            {
                return null;
            }
        }

        protected override object AddScalar(object inputValue, object outputValue)
        {
            long input;
            if (inputValue.GetType() == typeof(int))
            {
                input = (int)inputValue;
            }
            else
            {
                input = (long)inputValue;
            }

            if (outputValue == null)
            {
                return input;
            }

            return Math.Max((long)outputValue, input);
        }
    }
}