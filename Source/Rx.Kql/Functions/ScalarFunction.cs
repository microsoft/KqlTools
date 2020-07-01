// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Text;

    public abstract class ScalarFunction : ScalarValue
    {
        public string Name { get; set; }

        public List<ScalarValue> Arguments { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Name);
            sb.Append('(');

            string prefix = string.Empty;
            foreach (var a in Arguments)
            {
                sb.Append(prefix);
                prefix = ", ";
                sb.Append(a.ToString());
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}