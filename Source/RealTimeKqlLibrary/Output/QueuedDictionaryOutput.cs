using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public class QueuedDictionaryOutput : IOutput
    {
        public ConcurrentQueue<IDictionary<string, object>> KqlOutput;
        public Exception Error;

        private bool _running;
        private const int MAX_EVENTS = 1000000;

        public QueuedDictionaryOutput()
        {
            KqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
            _running = true;
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if(_running)
            {
                if(KqlOutput.Count > MAX_EVENTS)
                {
                    // Clear queue if user hasn't tried to dequeue last MAX_EVENTS events
                    KqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
                }
                try
                {
                    KqlOutput.Enqueue(obj);
                }
                catch (Exception ex)
                {
                    OutputError(ex);
                }
            }
        }

        public void OutputError(Exception ex)
        {
            _running = false;
            Error = ex;
        }

        public void Stop()
        {
            _running = false;
        }
    }
}