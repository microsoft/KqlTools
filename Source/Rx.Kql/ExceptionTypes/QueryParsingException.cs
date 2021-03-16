// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class QueryParsingException : Exception
    {
        public QueryParsingException()
        {
        }

        public QueryParsingException(string message)
            : base(message)
        {
        }

        public QueryParsingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}