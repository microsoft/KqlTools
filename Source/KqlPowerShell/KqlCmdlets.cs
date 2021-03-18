using System.Management.Automation;
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
            _eventComponent = new CsvFileReader(FilePath, this, Query);
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
            _eventComponent = new EtwSession(SessionName, this, Query);
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
            _eventComponent = new EtlFileReader(FilePath, this, Query);
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
            _eventComponent = new WinlogRealTime(LogName, this, Query);
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
            _eventComponent = new EvtxFileReader(FilePath, this, Query);
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
            _eventComponent = new SyslogFileReader(FilePath, this, Query);
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
            _eventComponent = new SyslogServer(NetworkAdapterName, UdpPort, this, Query);
        }
    }
}
