using System;
using System.Collections.Generic;
using System.Threading;
using RealTimeKqlLibrary;

namespace RealTimeKql
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Real-Time KQL!");

            // Parsing command line arguments
            var commandLineParser = new CommandLineParser(args);
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
            var query = commandLineParser.Query.Value;
            switch (commandLineParser.InputSubcommand.Name)
            {
                case "etw":
                    eventComponent = new EtwSession(arg, output, query);
                    break;
                case "etl":
                    eventComponent = new EtlFileReader(arg, output, query);
                    break;
                case "winlog":
                    eventComponent = new WinlogRealTime(arg, output, query);
                    break;
                case "evtx":
                    eventComponent = new EvtxFileReader(arg, output, query);
                    break;
                case "csv":
                    eventComponent = new CsvFileReader(arg, output, query);
                    break;
                case "syslog":
                    eventComponent = new SyslogFileReader(arg, output, query);
                    break;
                case "syslogserver":
                    eventComponent = GetSyslogServer(options, output, query);
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
            bool resetTable = false;
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
                    case "adxresettable":
                        resetTable = opt.WasSet;
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
                resetTable,
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

        static SyslogServer GetSyslogServer(List<Option> opts, IOutput output, string query)
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
            return new SyslogServer(networkAdapter, port, output, query);
        }
    }
}