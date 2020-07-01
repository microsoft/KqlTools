// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class UnknownFunctionException : Exception
    {
        public UnknownFunctionException()
        {
        }

        public UnknownFunctionException(string message) : base(message)
        {
        }

        public UnknownFunctionException(string message, Exception inner) 
            : base(message, inner)
        {
        }
    }
}
