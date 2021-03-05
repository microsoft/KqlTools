// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class TokenizerException : Exception
    {
        public TokenizerException()
        {
        }

        public TokenizerException(string message)
            : base(message)
        {
        }

        public TokenizerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}