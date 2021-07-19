using RealTimeKql;
using System.Collections.Generic;
using Xunit;

namespace RealTimeKqlTests
{
    public class CommandLineParserTest
    {
        [Theory]
        [InlineData("etw", "tcp")]
        [InlineData("etl", "Assets\\SampleEtl.etl")]
        [InlineData("winlog", "Security")]
        [InlineData("evtx", "Assets\\SampleEvtx.evtx")]
        [InlineData("csv", "Assets\\SampleCsv.csv")]
        [InlineData("syslog", "Assets\\SampleSyslog.txt")]
        [InlineData("syslogserver")]
        public void Parse_ValidInputs_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("syslogserver", "--udpport=514", "--networkadapter=value")]
        public void Parse_ValidInputWithOptions_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw")]
        [InlineData("etl")]
        [InlineData("winlog")]
        [InlineData("evtx")]
        [InlineData("csv")]
        public void Parse_InputsMissingRequirements_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("fake", "tcp")]
        [InlineData("fake", "Assets\\SampleEtl.etl")]
        [InlineData("fake")]
        public void Parse_FakeInputs_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json")]
        [InlineData("etw", "tcp", "table")]
        [InlineData("etw", "tcp", "adx", "--adxcluster=cluster", "--adxtable=table", "--adxdatabase=database")]
        [InlineData("etw", "tcp", "blob", "--blobconnectionstring=value", "--blobcontainername=value")]
        public void Parse_ValidOutputs_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "file.json")]
        [InlineData("etw", "tcp", "adx", "-ad=test.com", "-aclid=val", "-akey=val", "-acl=cluster", "-adb=database", "-atb=table", "-acr", "-adi")]
        public void Parse_ValidOutputsWithOptions_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "file.json")]
        [InlineData("etw", "tcp", "json", "file.json", "--query=test.kql")]
        [InlineData("etw", "tcp", "json", "file.json", "--query=test.kql", "test2.kql")]
        public void Parse_ValidOutputs_CheckValue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var expected = "file.json";

            c.Parse();
            var actual = c.OutputSubcommand.Argument.Value;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "adx", "--adxdatabase=database")]
        [InlineData("etw", "tcp", "blob", "--blobcontainername=value")]
        public void Parse_OutputsMissingRequirements_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "fake")]
        [InlineData("etw", "tcp", "fake", "--adxcluster=cluster", "--adxtable=table", "--adxdatabase=database")]
        [InlineData("etw", "tcp", "fake", "--blobconnectionstring=value", "--blobcontainername=value")]
        public void Parse_FakeOutputs_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "--query", "Assets\\test.kql")]
        [InlineData("etw", "tcp", "--query=Assets\\test.kql")]
        [InlineData("etw", "tcp", "--query", "Assets\\test.kql", "Assets\\test2.kql")]
        [InlineData("etw", "tcp", "-q", "Assets\\test.kql", "Assets\\test2.kql", "third.kql")]
        [InlineData("etw", "tcp", "--query=Assets\\test.kql", "Assets\\test2.kql")]
        [InlineData("etw", "tcp", "--query=Assets\\test.kql", "Assets\\test2.kql", "third.kql")]
        public void Parse_ValidQuery_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "--query")]
        [InlineData("etw", "tcp", "--query:Assets\\test.kql")]
        public void Parse_InvalidQuery_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "--query=test.kql")]
        [InlineData("etw", "tcp", "--query=test.kql")]
        [InlineData("etw", "tcp", "json")]
        public void Parse_ValidArgPositions_ReturnTrue(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "--query=test.kql", "json")]
        [InlineData("--query=test.kql", "etw", "tcp")]
        [InlineData("json", "etw", "tcp")]
        [InlineData("etw", "json", "tcp")]
        public void Parse_InvalidArgPositions_ReturnFalse(params string[] args)
        {
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Fact]
        public void Parse_OptAfterScmbWithOpts_ReturnTrue()
        {
            var args = new string[] { "etw", "tcp", "adx", "--adxcluster=cluster", "--adxdatabase=db", "--adxtable=tb", "--query=test.kql" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Fact]
        public void Parse_InputSubcommandWithArg()
        {
            var args = new string[] { "etw", "tcp" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("etw", c.InputSubcommand.Name);
            Assert.Equal("tcp", c.InputSubcommand.Argument.Value);
        }

        [Fact]
        public void Parse_InputSubcommandWithOpts()
        {
            var args = new string[] { "syslogserver", "--networkadapter", "all", "--udpport", "123" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("syslogserver", c.InputSubcommand.Name);
            foreach (var opt in c.InputSubcommand.Options)
            {
                switch (opt.LongName)
                {
                    case "networkadapter":
                        Assert.Equal("all", opt.Value);
                        break;
                    case "udpport":
                        Assert.Equal("123", opt.Value);
                        break;
                    default:
                        // There shouldn't be any other options, fail test if there is
                        Assert.Equal("test", "fail");
                        break;
                }
            }
        }

        [Fact]
        public void Parse_OutputSubcommandWithArg()
        {
            var args = new string[] { "etw", "tcp", "json", "file.json" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("json", c.OutputSubcommand.Name);
            Assert.Equal("file.json", c.OutputSubcommand.Argument.Value);
        }

        [Fact]
        public void Parse_OutputSubcommandWithOpts()
        {
            var args = new string[] { "etw", "tcp", "adx", "-ad=test.com", "-aclid=val", "-akey=val", "-acl=cluster", "-adb=database", "-atb=table", "-acr", "-adi" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("adx", c.OutputSubcommand.Name);
            foreach (var opt in c.OutputSubcommand.Options)
            {
                switch (opt.LongName)
                {
                    case "adxauthority":
                        Assert.Equal("test.com", opt.Value);
                        break;
                    case "adxclientid":
                        Assert.Equal("val", opt.Value);
                        break;
                    case "adxkey":
                        Assert.Equal("val", opt.Value);
                        break;
                    case "adxcluster":
                        Assert.Equal("cluster", opt.Value);
                        break;
                    case "adxdatabase":
                        Assert.Equal("database", opt.Value);
                        break;
                    case "adxtable":
                        Assert.Equal("table", opt.Value);
                        break;
                    case "adxdirectingest":
                        Assert.True(opt.WasSet);
                        break;
                    case "adxcreatereset":
                        Assert.True(opt.WasSet);
                        break;
                    default:
                        // There shouldn't be any other options, fail test if there is
                        Assert.Equal("test", "fail");
                        break;
                }
            }
        }

        [Fact]
        public void Parse_Queries()
        {
            var args = new string[] { "etw", "tcp", "-q", "Assets\\test.kql", "Assets\\test2.kql", "third.kql" };
            var inputs = GetAllInputs();
            var outputs = GetAllOutputs();
            var query = GetQueryOption();
            var c = new CommandLineParser(inputs, outputs, query, args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Contains("Assets\\test.kql", c.Queries);
            Assert.Contains("Assets\\test2.kql", c.Queries);
            Assert.Contains("third.kql", c.Queries);
        }

        private List<Subcommand> GetAllInputs()
        {
            var inputs = new List<Subcommand>();

            // etw
            var sessionName = new Argument("session", "Name of the ETW Session to attach to", true);
            var etw = new Subcommand("etw", "Listen to real-time ETW session. See Event Trace Sessions in Perfmon", sessionName);
            inputs.Add(etw);

            // etl
            var etlFile = new Argument("file.etl", "Path to the .etl file to read", true);
            var etl = new Subcommand("etl", "Process the past event in Event Trace File (.etl) recorded via ETW", etlFile);
            inputs.Add(etl);

            // log
            var logName = new Argument("logname", "Name of the Windows log to attach to", true);
            var winlog = new Subcommand("winlog", "Listen for new events in a Windows OS log. See Windows Logs in Eventvwr", logName);
            inputs.Add(winlog);

            // evtx
            var evtxFile = new Argument("file.evtx", "Path to the .evtx file to read", true);
            var evtx = new Subcommand("evtx", "Process the past events recorded in Windows log file on disk", evtxFile);
            inputs.Add(evtx);

            // csv
            var csvFile = new Argument("file.csv", "Path to the .csv file to read", true);
            var csv = new Subcommand("csv", "Process past events recorded in Comma Separated File", csvFile);
            inputs.Add(csv);

            // syslog
            var syslogFile = new Argument("filepath", "Path to the log file to read", true);
            var syslog = new Subcommand("syslog", "Process real-time syslog messages written to local log file", syslogFile);
            inputs.Add(syslog);

            // syslog server
            var syslogServerOptions = new List<Option>()
            {
                new Option("networkadapter", "na",
                "Network Adapter Name. Optional, when not specified, listner listens on all adapters. Used along with UDP Port."),
                new Option("udpport", "p",
                "Optional. Listen to a UDP port for syslog messages. Default is port 514. eg, --udpport=514.")
                {
                    Value = "514"
                }
            };
            var syslogServer = new Subcommand("syslogserver", "Listen to syslog messages on a UDP port", null, syslogServerOptions);
            inputs.Add(syslogServer);

            return inputs;

        }

        private List<Subcommand> GetAllOutputs()
        {
            var outputs = new List<Subcommand>();

            // json
            var jsonFile = new Argument("file.json", "The path to the .json file to write to");
            var json = new Subcommand("json",
                "Optional and default. Events printed to console in JSON format. If filename is specified immediately after, events will be written to the file in JSON format.",
                jsonFile);
            outputs.Add(json);

            // table
            var table = new Subcommand("table", "Optional, events printed to console in table format");
            outputs.Add(table);

            // adx
            var adxOptions = new List<Option>()
            {
                new Option("adxauthority", "ad",
                "Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com")
                {
                    Value = "microsoft.com"
                },
                new Option("adxclientid", "aclid",
                "Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer."),
                new Option("adxkey", "akey",
                "Azure Data Explorer (ADX) Access Key. Used along with ClientId"),
                new Option("adxcluster", "acl",
                "Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net", true),
                new Option("adxdatabase", "adb",
                "Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb", true),
                new Option("adxtable", "atb",
                "Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable", true),
                new Option("adxcreatereset", "acr",
                "If table doesn't exist, it is created. If table exists, data in table is dropped before new data is logged. eg, --adxcreatereset", false, true),
                new Option("adxdirectingest", "adi",
                "Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX. eg, --adxdirectingest", false, true)
            };

            var adx = new Subcommand("adx", "Ingest output to Azure Data Explorer", null, adxOptions);
            outputs.Add(adx);

            // blob
            var blobOptions = new List<Option>()
            {
                new Option("blobconnectionstring", "bcs", "Azure Blob Storage Connection string.", true, false),
                new Option("blobcontainername", "bcn", "Azure Blob Storage container name.", true, false)
            };

            var blob = new Subcommand("blob", "Ingest output to Azure Blob Storage", null, blobOptions);
            outputs.Add(blob);

            return outputs;
        }

        private Option GetQueryOption()
        {
            return new Option("query", "q",
                "Optional, apply this KQL query to the input stream. If omitted, the stream is propagated without processing to the output. eg, --query=file.kql");
        }

    }
}
