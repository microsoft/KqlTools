// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
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
