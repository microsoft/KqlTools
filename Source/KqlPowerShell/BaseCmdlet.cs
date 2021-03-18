using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using RealTimeKqlLibrary;

namespace KqlPowerShell
{
    public abstract class BaseCmdlet : Cmdlet, IOutput
    {
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Query;

        protected EventComponent _eventComponent;

        private ConcurrentQueue<IDictionary<string, object>> _kqlOutput;
        private Exception _error;
        private bool _running = false;
        private const int MAX_EVENTS = 1000000;

        // Set up event component in derived class
        protected abstract void SetupProcessing();

        protected override void BeginProcessing()
        {
            _kqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
            SetupProcessing();

            if (_eventComponent == null || !_eventComponent.Start())
            {
                WriteWarning($"ERROR! Problem starting up.");
                // TODO: exit gracefully here
            }
            else
            {
                _running = true;
            }
        }

        protected override void ProcessRecord()
        {
            while (_running)
            {
                int eventsPrinted = 0;
                if (_kqlOutput.TryDequeue(out var eventOutput))
                {
                    PSObject row = new PSObject();
                    foreach (var pair in eventOutput)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }

                    WriteObject(row);
                    eventsPrinted++;
                }

                if (_error != null)
                {
                    WriteWarning(_error.Message);
                    break;
                }

                if (eventsPrinted % 10 == 0) { Thread.Sleep(20); }
            }
        }

        protected override void EndProcessing()
        {
            while (_kqlOutput.TryDequeue(out var eventOutput))
            {
                PSObject row = new PSObject();
                foreach (var pair in eventOutput)
                {
                    row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                }

                WriteObject(row);
            }

            if (_error != null) WriteWarning(_error.Message);
            WriteObject("\nCompleted!\nThank you for using Real-Time KQL!");
        }

        public void KqlOutputAction(System.Reactive.Kql.CustomTypes.KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_running)
            {
                if (_kqlOutput.Count > MAX_EVENTS)
                {
                    // Clear queue if user hasn't tried to dequeue last MAX_EVENTS events
                    _kqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
                }
                try
                {
                    _kqlOutput.Enqueue(obj);
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
            _error = ex;
        }

        public void OutputCompleted()
        {
            _running = false;
        }

        public void Stop()
        {
            EndProcessing();
        }
    }
}
