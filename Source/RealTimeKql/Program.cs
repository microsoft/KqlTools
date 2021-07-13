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
            // Setting up logging
            BaseLogger logger;
#if NET472
            logger = new WindowsLogger("RealTimeKqlLogging", "RealTimeKql");
#else
            logger = new ConsoleLogger();
#endif

            logger.Log(LogLevel.INFORMATION, "Welcome to Real-Time KQL!");

            // Parsing command line arguments
            var commandLineParser = new CommandLineParser(args);
            if (!commandLineParser.Parse())
            {
                logger.Log(LogLevel.ERROR, "Problem parsing command line arguments. Terminating program...");
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
                output = new ConsoleJsonOutput(logger);
            }
            switch (commandLineParser.OutputSubcommand.Name)
            {
                case "json":
                    output = GetJsonOutput(logger, commandLineParser.OutputSubcommand);
                    break;
                case "table":
                    output = new ConsoleTableOutput(logger);
                    break;
                case "adx":
                    output = GetAdxOutput(logger, commandLineParser.OutputSubcommand.Options);
                    break;
                case "blob":
                    output = GetBlobOutput(logger, commandLineParser.OutputSubcommand.Options);
                    break;
                default:
                    logger.Log(LogLevel.ERROR, $"ERROR! Problem recognizing output method specified: {commandLineParser.OutputSubcommand.Name}");
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
                    logger.Log(LogLevel.ERROR, $"Problem recognizing input method specified: {commandLineParser.InputSubcommand.Name}. Terminating program...");
                    return;
            }
            if (!eventComponent.Start())
            {
                logger.Log(LogLevel.ERROR, "Error starting up. Please review usage and examples. Terminating program...");
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

        static IOutput GetJsonOutput(BaseLogger logger, Subcommand subcommand)
        {
            if(subcommand.Argument.Value != null)
            {
                return new JsonFileOutput(logger, subcommand.Argument.Value);
            }

            return new ConsoleJsonOutput(logger);
        }

        // Generates adx output object based off passed in options
        static AdxOutput GetAdxOutput(BaseLogger logger, List<Option> opts)
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
                logger,
                auth,
                clientId,
                key,
                cluster,
                database,
                table,
                createOrResetTable,
                directIngest);
        }

        static BlobOutput GetBlobOutput(BaseLogger logger, List<Option> opts)
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

            return new BlobOutput(logger, connectionString, containerName);
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
