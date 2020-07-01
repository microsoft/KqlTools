// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    //TODO unit tests
    class ScalarPath : ScalarValue
    {
        public ScalarValue Element { get; set; }
        public ScalarValue Selector { get; set; }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftValue = Element.GetValue(evt);

            if (leftValue == null)
            {
                return null;
            }

            var selector = Selector;
            if (selector is ScalarConst) //handle a.["b"], treat "b" as a property
            {
                selector = new ScalarProperty(((ScalarConst)Selector).Value.ToString());
            }

            if (leftValue.IsDynamicObject() || leftValue.IsGenericCollectionObject())
            {
                return selector.GetValue((IDictionary<string, object>)leftValue);
            }

            if (leftValue.IsGenericObject())
            {
                var dict = leftValue.GetType()
                    .GetProperties()
                    .ToDictionary(x => x.Name, x => x.GetValue(leftValue));
                return selector.GetValue(dict);
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return $"{Element}.{Selector}";
        }
    }
}
