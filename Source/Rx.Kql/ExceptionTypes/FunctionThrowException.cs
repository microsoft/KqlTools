// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class FunctionThrowException : Exception
    {
        public FunctionThrowException()
        {
        }

        public FunctionThrowException(string message)
            : base(message)
        {
        }

        public FunctionThrowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
