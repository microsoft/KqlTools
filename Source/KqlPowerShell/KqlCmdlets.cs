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

        protected override EventComponent SetupEventComponent()
        {
            return new CsvFileReader(FilePath, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new EtwSession(SessionName, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new EtlFileReader(FilePath, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new WinlogRealTime(LogName, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new EvtxFileReader(FilePath, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new SyslogFileReader(FilePath, _output, Query);
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

        protected override EventComponent SetupEventComponent()
        {
            return new SyslogServer(NetworkAdapterName, UdpPort, _output, Query);
        }
    }
}
