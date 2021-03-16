// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class SummarizeOperatorException : Exception
    {
        public SummarizeOperatorException()
        {
        }

        public SummarizeOperatorException(string message)
            : base(message)
        {
        }

        public SummarizeOperatorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}