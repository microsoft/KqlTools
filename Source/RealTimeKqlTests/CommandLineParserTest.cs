using RealTimeKql;
using System;
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("syslogserver", "--udpport=514", "--networkadapter=value")]
        public void Parse_ValidInputWithOptions_ReturnTrue(params string[] args)
        {
            var c = new CommandLineParser(args);
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("fake", "tcp")]
        [InlineData("fake", "Assets\\SampleEtl.etl")]
        [InlineData("fake")]
        public void Parse_FakeInputs_ReturnFalse(params string[] args)
        {
            var c = new CommandLineParser(args);
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "file.json")]
        [InlineData("etw", "tcp", "adx", "-ad=test.com", "-aclid=val", "-akey=val", "-acl=cluster", "-adb=database", "-atb=table", "-acr", "-adi")]
        public void Parse_ValidOutputsWithOptions_ReturnTrue(params string[] args)
        {
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "file.json")]
        [InlineData("etw", "tcp", "json", "file.json", "--query=test.kql")]
        [InlineData("etw", "tcp", "json", "file.json", "--query=test.kql", "test2.kql")]
        public void Parse_ValidOutputs_CheckValue(params string[] args)
        {
            var c = new CommandLineParser(args);
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "fake")]
        [InlineData("etw", "tcp", "fake", "--adxcluster=cluster", "--adxtable=table", "--adxdatabase=database")]
        [InlineData("etw", "tcp", "fake", "--blobconnectionstring=value", "--blobcontainername=value")]
        public void Parse_FakeOutputs_ReturnFalse(params string[] args)
        {
            var c = new CommandLineParser(args);
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "--query")]
        [InlineData("etw", "tcp", "--query:Assets\\test.kql")]
        public void Parse_InvalidQuery_ReturnFalse(params string[] args)
        {
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Theory]
        [InlineData("etw", "tcp", "json", "--query=test.kql")]
        [InlineData("etw", "tcp", "--query=test.kql")]
        [InlineData("etw", "tcp", "json")]
        public void Parse_ValidArgPositions_ReturnTrue(params string[] args)
        {
            var c = new CommandLineParser(args);
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
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.False(actual);
        }

        [Fact]
        public void Parse_OptAfterScmbWithOpts_ReturnTrue()
        {
            var args = new string[] { "etw", "tcp", "adx", "--adxcluster=cluster", "--adxdatabase=db", "--adxtable=tb", "--query=test.kql" };
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
        }

        [Fact]
        public void Parse_InputSubcommandWithArg()
        {
            var args = new string[] { "etw", "tcp" };
            var c = new CommandLineParser(args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("etw", c.InputSubcommand.Name);
            Assert.Equal("tcp", c.InputSubcommand.Argument.Value);
        }

        [Fact]
        public void Parse_InputSubcommandWithOpts()
        {
            var args = new string[] { "syslogserver", "--networkadapter", "all", "--udpport", "123" };
            var c = new CommandLineParser(args);

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
            var c = new CommandLineParser(args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Equal("json", c.OutputSubcommand.Name);
            Assert.Equal("file.json", c.OutputSubcommand.Argument.Value);
        }

        [Fact]
        public void Parse_OutputSubcommandWithOpts()
        {
            var args = new string[] { "etw", "tcp", "adx", "-ad=test.com", "-aclid=val", "-akey=val", "-acl=cluster", "-adb=database", "-atb=table", "-acr", "-adi" };
            var c = new CommandLineParser(args);

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
            var c = new CommandLineParser(args);

            var success = c.Parse();

            Assert.True(success);
            Assert.Contains("Assets\\test.kql", c.Queries);
            Assert.Contains("Assets\\test2.kql", c.Queries);
            Assert.Contains("third.kql", c.Queries);
        }

    }
}
