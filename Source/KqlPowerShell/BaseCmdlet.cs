using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using RealTimeKqlLibrary;

namespace KqlPowerShell
{
    public abstract class BaseCmdlet : Cmdlet
    {
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Queries;

        protected EventComponent _eventComponent;
        protected QueuedDictionaryOutput _output;
        private bool _startupError = false;

        // Set up event component in derived class
        protected abstract EventComponent SetupEventComponent();

        protected override void BeginProcessing()
        {
            // Instantiating output component
            _output = new QueuedDictionaryOutput();

            // Let child class instantiate event component
            _eventComponent = SetupEventComponent();

            // Starting up event component
            if (_eventComponent == null || !_eventComponent.Start())
            {
                WriteWarning($"ERROR! Problem starting up.");
                _startupError = true;
            }
        }

        protected override void ProcessRecord()
        {
            if (_startupError) return;

            // Loop runs while output is running or queue is not empty
            while (_output.Running || !_output.KqlOutput.IsEmpty)
            {
                int eventsPrinted = 0;
                if (_output.KqlOutput.TryDequeue(out IDictionary<string, object> eventOutput))
                {
                    PSObject row = new PSObject();
                    foreach (var pair in eventOutput)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }
                    WriteObject(row);
                    Interlocked.Increment(ref eventsPrinted);
                }

                if (_output.Error != null)
                {
                    WriteWarning(_output.Error.Message);
                    break;
                }

                if (eventsPrinted % 10 == 0) { Thread.Sleep(20); }
            }
        }

        protected override void EndProcessing()
        {
            if (_output.Error != null) WriteWarning(_output.Error.Message);
            WriteObject("\nCompleted!\nThank you for using Real-Time KQL!");
        }

        protected override void StopProcessing()
        {
            EndProcessing();
        }
    }
}
