// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.EventTypes
{
    public class KqlQueryCompletedEventArgs : EventArgs
    {
        public DateTime CompletionDateTime { get; set; } = DateTime.UtcNow;

        public string Message { get; set; }

        public string Comment { get; set; }

        public string Query { get; set; }
    }
}