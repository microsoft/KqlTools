using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;
using RealTimeKqlLibrary;

namespace KqlPowerShell
{
    public class QueuedDictionaryOutput : IOutput
    {
        public ConcurrentQueue<IDictionary<string, object>> KqlOutput;
        public Exception Error { get; private set; }
        public bool Running { get; private set; }
        private const int MAX_EVENTS = 1000000;

        public QueuedDictionaryOutput()
        {
            KqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
            Running = true;
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if(Running)
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
            // Stop enqueueing new events
            Running = false;
            Error = ex;
        }

        public void OutputCompleted()
        {
            // Stop enqueueing new events
            Running = false;
        }

        public void Stop()
        {
            // can add code to exit process here if needed
        }
    }
}
