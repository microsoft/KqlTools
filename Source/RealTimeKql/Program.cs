using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RealTimeKqlLibrary;

namespace RealTimeKql
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Real-Time KQL!");

            // Setting up command line and config file parsers
            var inputSubcommands = new List<Subcommand>();
            var outputSubcommands = new List<Subcommand>();
            var query = new Option("query", "q",
                "Optional, apply this KQL query to the input stream. If omitted, the stream is propagated without processing to the output. eg, --query=file.kql");

            // Add subcommands according to OS type
            // Input subcommands
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // etw
                var sessionName = new Argument("session", "Name of the ETW Session to attach to", true);
                var etw = new Subcommand("etw", "Listen to real-time ETW session. See Event Trace Sessions in Perfmon", sessionName);
                inputSubcommands.Add(etw);

                // etl
                var etlFile = new Argument("file.etl", "Path to the .etl file to read", true);
                var etl = new Subcommand("etl", "Process the past event in Event Trace File (.etl) recorded via ETW", etlFile);
                inputSubcommands.Add(etl);

                // log
                var logName = new Argument("logname", "Name of the Windows log to attach to", true);
                var winlog = new Subcommand("winlog", "Listen for new events in a Windows OS log. See Windows Logs in Eventvwr", logName);
                inputSubcommands.Add(winlog);

                // evtx
                var evtxFile = new Argument("file.evtx", "Path to the .evtx file to read", true);
                var evtx = new Subcommand("evtx", "Process the past events recorded in Windows log file on disk", evtxFile);
                inputSubcommands.Add(evtx);
            }
            else
            {
                // syslog
                var syslogFile = new Argument("filepath", "Path to the log file to read", true);
                var syslog = new Subcommand("syslog", "Process real-time syslog messages written to local log file", syslogFile);
                inputSubcommands.Add(syslog);

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
                inputSubcommands.Add(syslogServer);
            }

            // csv
            var csvFile = new Argument("file.csv", "Path to the .csv file to read", true);
            var csv = new Subcommand("csv", "Process past events recorded in Comma Separated File", csvFile);
            inputSubcommands.Add(csv);

            // Parsing command line arguments
            var commandLineParser = new CommandLineParser(inputSubcommands, outputSubcommands, query, args);
            if (!commandLineParser.Parse())
            {
                Console.WriteLine("Problem parsing command line arguments. Terminating program...");
                return;
            }
            else if(commandLineParser.InputSubcommand == null)
            {
                // User called help, terminating program
                return;
            }

            // Output subcommands
            // json
            var jsonFile = new Argument("file.json", "The path to the .json file to write to");
            var json = new Subcommand("json",
                "Optional and default. Events printed to console in JSON format. If filename is specified immediately after, events will be written to the file in JSON format.",
                jsonFile);
            outputSubcommands.Add(json);

            // table
            var table = new Subcommand("table", "Optional, events printed to console in table format");
            outputSubcommands.Add(table);

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
            outputSubcommands.Add(adx);

            // blob
            var blobOptions = new List<Option>()
            {
                new Option("blobconnectionstring", "bcs", "Azure Blob Storage Connection string.", true, false),
                new Option("blobcontainername", "bcn", "Azure Blob Storage container name.", true, false)
            };

            var blob = new Subcommand("blob", "Ingest output to Azure Blob Storage", null, blobOptions);
            outputSubcommands.Add(blob);

            // Setting up output method
            IOutput output = null;
            if(commandLineParser.OutputSubcommand == null)
            {
                output = new ConsoleJsonOutput();
            }
            switch (commandLineParser.OutputSubcommand.Name)
            {
                case "json":
                    output = GetJsonOutput(commandLineParser.OutputSubcommand);
                    break;
                case "table":
                    output = new ConsoleTableOutput();
                    break;
                case "adx":
                    output = GetAdxOutput(commandLineParser.OutputSubcommand.Options);
                    break;
                case "blob":
                    output = GetBlobOutput(commandLineParser.OutputSubcommand.Options);
                    break;
                default:
                    Console.WriteLine($"ERROR! Problem recognizing output method specified: {commandLineParser.OutputSubcommand.Name}");
                    return;
            }

            // Setting up event component
            EventComponent eventComponent = null;
            var arg = commandLineParser.InputSubcommand.Argument?.Value;
            var options = commandLineParser.InputSubcommand.Options;
            var queries = commandLineParser.Queries.ToArray();
            switch (commandLineParser.InputSubcommand.Name)
            {
                case "etw":
                    eventComponent = new EtwSession(arg, output, queries);
                    break;
                case "etl":
                    eventComponent = new EtlFileReader(arg, output, queries);
                    break;
                case "winlog":
                    eventComponent = new WinlogRealTime(arg, output, queries);
                    break;
                case "evtx":
                    eventComponent = new EvtxFileReader(arg, output, queries);
                    break;
                case "csv":
                    eventComponent = new CsvFileReader(arg, output, queries);
                    break;
                case "syslog":
                    eventComponent = new SyslogFileReader(arg, output, queries);
                    break;
                case "syslogserver":
                    eventComponent = GetSyslogServer(options, output, queries);
                    break;
                default:
                    Console.WriteLine($"Problem recognizing input method specified: {commandLineParser.InputSubcommand.Name}. Terminating program...");
                    return;
            }
            if (!eventComponent.Start())
            {
                Console.WriteLine("Error starting up. Please review usage and examples. Terminating program...");
                return;
            }

            // Waiting for exit signal
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = false;
                exitEvent.Set();
            };
            exitEvent.WaitOne();
        }

        static IOutput GetJsonOutput(Subcommand subcommand)
        {
            if(subcommand.Argument.Value != null)
            {
                return new JsonFileOutput(subcommand.Argument.Value);
            }

            return new ConsoleJsonOutput();
        }

        // Generates adx output object based off passed in options
        static AdxOutput GetAdxOutput(List<Option> opts)
        {
            string auth = "";
            string clientId = "";
            string key = "";
            string cluster = "";
            string database = "";
            string table = "";
            bool createOrResetTable = false;
            bool directIngest = false;

            foreach(var opt in opts)
            {
                switch(opt.LongName)
                {
                    case "adxauthority":
                        auth = opt.Value;
                        break;
                    case "adxclientid":
                        clientId = opt.Value;
                        break;
                    case "adxkey":
                        key = opt.Value;
                        break;
                    case "adxcluster":
                        cluster = opt.Value;
                        break;
                    case "adxdatabase":
                        database = opt.Value;
                        break;
                    case "adxtable":
                        table = opt.Value;
                        break;
                    case "adxdirectingest":
                        directIngest = opt.WasSet;
                        break;
                    case "adxcreatereset":
                        createOrResetTable = opt.WasSet;
                        break;
                }
            }

            return new AdxOutput(
                auth,
                clientId,
                key,
                cluster,
                database,
                table,
                createOrResetTable,
                directIngest);
        }

        static BlobOutput GetBlobOutput(List<Option> opts)
        {
            string connectionString = "";
            string containerName = "";

            foreach (var opt in opts)
            {
                switch (opt.LongName)
                {
                    case "blobconnectionstring":
                        connectionString = opt.Value;
                        break;
                    case "blobcontainername":
                        containerName = opt.Value;
                        break;
                }
            }

            return new BlobOutput(connectionString, containerName);
        }

        static SyslogServer GetSyslogServer(List<Option> opts, IOutput output, string[] queries)
        {
            string networkAdapter = "";
            string udpport = "";

            foreach(var opt in opts)
            {
                switch(opt.LongName)
                {
                    case "networkadapter":
                        networkAdapter = opt.Value;
                        break;
                    case "udpport":
                        udpport = opt.Value;
                        break;
                }
            }

            if (!int.TryParse(udpport, out var port))
            {
                port = 514;
            }
            return new SyslogServer(networkAdapter, port, output, queries);
        }
    }
}