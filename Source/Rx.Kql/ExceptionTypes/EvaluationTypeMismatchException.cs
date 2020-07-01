// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.ExceptionTypes
{
    public class EvaluationTypeMismatchException : Exception
    {
        public EvaluationTypeMismatchException()
        {
        }

        public EvaluationTypeMismatchException(string message)
            : base(message)
        {
        }

        public EvaluationTypeMismatchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}