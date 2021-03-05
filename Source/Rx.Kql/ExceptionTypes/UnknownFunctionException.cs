// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
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
