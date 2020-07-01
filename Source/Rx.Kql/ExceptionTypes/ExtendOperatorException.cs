// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class ExtendOperatorException : Exception
    {
        public ExtendOperatorException()
        {
        }

        public ExtendOperatorException(string message)
            : base(message)
        {
        }

        public ExtendOperatorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}