// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;

    [Serializable]
    public class ScalarSummarizer : ScalarValue
    {
        public Dictionary<string, AggregationFunction> Aggregations { get; set; } = new Dictionary<string, AggregationFunction>();

        public ScalarValueList GroupingElements { get; set; }

        public override object GetValue(IDictionary<string, object> currentItem)
        {
            throw new NotImplementedException();
        }
    }
}
