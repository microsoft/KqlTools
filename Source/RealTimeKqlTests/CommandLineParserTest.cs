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
        [InlineData("etw", "tcp", "adx", "-ad=test.com", "-aclid=val", "-akey=val", "-acl=cluster", "-adb=database", "-atb=table", "-art", "-adi")]
        public void Parse_ValidOutputsWithOptions_ReturnTrue(params string[] args)
        {
            var c = new CommandLineParser(args);
            var actual = c.Parse();
            Assert.True(actual);
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
    }
}
