using System.Management.Automation;
using System.Threading;
using RealTimeKqlLibrary;

namespace KqlPowerShell
{
    [Cmdlet(VerbsCommon.Get, "CsvFileReader")]
    public class GetCsvFileReader : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string FilePath;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new CsvFileReader(FilePath, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "EtwSession")]
    public class GetEtwSession : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string SessionName;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new EtwSession(SessionName, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "EtlFileReader")]
    public class GetEtlFileReader : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string FilePath;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new EtlFileReader(FilePath, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "WinlogRealTime")]
    public class GetWinlogRealTime : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string LogName;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new WinlogRealTime(LogName, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "EvtxFileReader")]
    public class GetEvtxFileReader : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string FilePath;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new EvtxFileReader(FilePath, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "SyslogFileReader")]
    public class GetSyslogFileReader : BaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string FilePath;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new SyslogFileReader(FilePath, _output, Query);
        }
    }

    [Cmdlet(VerbsCommon.Get, "SyslogServer")]
    public class GetSyslogServer : BaseCmdlet
    {
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string NetworkAdapterName;

        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public int UdpPort = 514;

        protected override void SetupProcessing()
        {
            _output = new QueuedDictionaryOutput();
            _eventComponent = new SyslogServer(NetworkAdapterName, UdpPort, _output, Query);
        }
    }

    public abstract class BaseCmdlet : Cmdlet
    {
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Query;

        protected EventComponent _eventComponent;
        protected QueuedDictionaryOutput _output;
        private bool _running = false;

        // Set up variables in derived class
        protected abstract void SetupProcessing();

        protected override void BeginProcessing()
        {
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
                if (_output.KqlOutput.TryDequeue(out var eventOutput))
                {
                    PSObject row = new PSObject();
                    foreach (var pair in eventOutput)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }

                    if (_running)
                    {
                        WriteObject(row);
                        eventsPrinted++;
                    }
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
