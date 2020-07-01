// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using Collections;
    using System.Collections.Generic;

    //TODO unit test IncidentSubscription.[strcat("0", a)]
    class ScalarIndexedElement : ScalarValue
    {
        public ScalarValue Element { get; set; }
        public ScalarValue Selector { get; set; }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Element.GetValue(evt);
            var indexer = Selector.GetValue(evt);

            if (value.IsGenericList() && RxKqlCommonFunctions.TryConvert<int>(indexer, out var index))
            {
                //TODO negative indexing
                var list = (IList)value;

                if (index < 0 || index >= list.Count)
                {
                    return string.Empty;
                }
                return list[index];
            }

            //dict[col] is equivalent to dict.col
            return new ScalarPath
            {
                Element = Element,
                Selector = Selector
            }.GetValue(evt);
        }
    }
}
