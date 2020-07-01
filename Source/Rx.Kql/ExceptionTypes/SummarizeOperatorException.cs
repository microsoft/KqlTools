// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
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