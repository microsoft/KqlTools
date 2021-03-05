// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class BooleanExpressionException : Exception
    {
        public BooleanExpressionException()
        {
        }

        public BooleanExpressionException(string message)
            : base(message)
        {
        }

        public BooleanExpressionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}