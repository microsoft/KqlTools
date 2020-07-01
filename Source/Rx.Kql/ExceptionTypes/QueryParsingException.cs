// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
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