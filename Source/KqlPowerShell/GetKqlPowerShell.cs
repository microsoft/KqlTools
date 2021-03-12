using RealTimeKqlLibrary;
using System.Management.Automation;
using System.Threading;

namespace KqlPowerShell
{
    [Cmdlet(VerbsCommon.Get, "KqlPowerShell")]
    public class GetKqlPowerShell : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string InputType;

        [Parameter(Mandatory = true, Position = 1)]
        public string InputArgument;

        [Parameter(Mandatory = false)]
        public string Query;

        private EventComponent _eventComponent;
        private QueuedDictionaryOutput _output;

        private bool _running;

        protected override void BeginProcessing()
        {
            // Init output component
            _output = new QueuedDictionaryOutput();

            // Init event component
            switch(InputType)
            {
                case "etw":
                    _eventComponent = new EtwSession(InputArgument, _output, Query);
                    break;
                case "etl":
                    _eventComponent = new EtlFileReader(InputArgument, _output, Query);
                    break;
                default:
                    WriteWarning($"ERROR! Could not find input type {InputType}.");
                    // TODO: exit gracefully here
                    break;
            }
        }

        protected override void ProcessRecord()
        {
            if (!_eventComponent.Start())
            {
                WriteWarning($"ERROR! Problem starting up {InputType}.");
                // TODO: exit gracefully here
            }
            else
            {
                _running = true;
            }

            while(_running)
            {
                int eventsPrinted = 0;
                if (_output.KqlOutput.TryDequeue(out var eventOutput))
                {
                    PSObject row = new PSObject();
                    foreach (var pair in eventOutput)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }

                    if(_running)
                    {
                        WriteObject(row);
                        eventsPrinted++;
                    }
                }

                if(_output.Error != null)
                {
                    WriteWarning(_output.Error.Message);
                    break;
                }

                if (eventsPrinted % 10 == 0) { Thread.Sleep(20); }
            }
        }

        protected override void EndProcessing()
        {
            _running = false;
            _eventComponent.Stop();
        }

        protected override void StopProcessing()
        {
            _running = false;
            _eventComponent.Stop();
        }
    }
}