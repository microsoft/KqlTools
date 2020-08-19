// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

// #define BUILT_FOR_WINDOWS Uncomment this line to get intellisense to work

#if BUILT_FOR_WINDOWS

namespace RealTimeKql
{
    using Kusto.Data;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    partial class Program
    {
        public static void InvokeEtw(CommandLineApplication command)
        {
            //description and help text of the command.
            command.Description = "Realtime filter of ETW Events";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter ETW events that are logged to the trace session. This option can also be used with ETL log file(s) on disk. Example is file(s) copied from another machine or previous ETW sessions." + Environment.NewLine
                + Environment.NewLine + "Real-time session"
                + Environment.NewLine + "\tRealtimeKql etw --session=tcp --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with ETW reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded ETL Trace Log (.etl files)"
                + Environment.NewLine + "\tRealtimeKql etw --filter=*.etl --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql etw --session=tcp --query=QueryFile.csl --outputjson=Tcp.json" + Environment.NewLine
                + Environment.NewLine + "Note: Logman can be used to start a ETW trace. In this example we are creating a trace session named tcp with Tcp Provider guid." + Environment.NewLine
                + Environment.NewLine + "\tlogman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets" + Environment.NewLine
                + Environment.NewLine + "When done, stopping the trace session is using command,"
                + Environment.NewLine + "\tlogman.exe stop tcp -ets" + Environment.NewLine
                + Environment.NewLine + "If you are getting the error Unexpected TDH status 1168, this indicates ERROR_NOT_FOUND."
                + Environment.NewLine + "Only users with administrative privileges, users in the Performance Log Users group, and applications running as LocalSystem, LocalService, NetworkService could do that. It could mean that you are running the application as someone who doesnt belong to the above mentioned user groups.To grant a restricted user the ability to consume events in real time, add them to the Performance Log Users group.";

            command.HelpOption("-?|-h|--help");

            // input
            var sessionOption = command.Option("-s|--session <value>",
                "Name of the ETW Session to attach to. eg, --session=tcp. tcp is the name of the session started using logman or such tools.",
                CommandOptionType.SingleValue);

            var filterPatternOption = command.Option("-f|--file <value>",
                "File pattern to filter files by. eg, --filter=*.etl",
                CommandOptionType.SingleValue);

            // query for real-time view or pre-processing
            var kqlQueryOption = command.Option("-q|--query <value>",
                "Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/",
                CommandOptionType.SingleValue);

            // output
            var browserLogOption = command.Option("-ob|--outputbrowser",
                "Log the output to browser.",
                CommandOptionType.NoValue);

            var consoleLogOption = command.Option("-oc|--outputconsole",
                "Log the output to console.",
                CommandOptionType.NoValue);

            var outputFileOption = command.Option("-oj|--outputjson <value>",
                "Write output to JSON file. eg, --outputjson=FilterOutput.json",
                CommandOptionType.SingleValue);

            var adAuthority = command.Option("-ad|--adxauthority <value>",
                "Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com",
                CommandOptionType.SingleValue);

            var adClientAppId = command.Option("-aclid|--adxclientid <value>",
                "Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.",
                CommandOptionType.SingleValue);

            var adKey = command.Option("-akey|--adxkey <value>",
                "Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id",
                CommandOptionType.SingleValue);

            var clusterAddressOption = command.Option("-ac|--adxcluster <value>",
                "Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net",
                CommandOptionType.SingleValue);

            var databaseOption = command.Option("-ad|--adxdatabase <value>",
                "Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb",
                CommandOptionType.SingleValue);

            var tableOption = command.Option("-at|--adxtable <value>",
                "Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable",
                CommandOptionType.SingleValue);

            var resetTableOption = command.Option("-ar|--adxreset",
                "The existing data in the destination table is dropped before new data is logged.",
                CommandOptionType.NoValue);

            var directIngestOption = command.Option("-ad|--adxdirect",
                "Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                KustoConnectionStringBuilder kscbIngest = null;
                KustoConnectionStringBuilder kscbAdmin = null;

                if (kqlQueryOption.HasValue() && !File.Exists(kqlQueryOption.Value()))
                {
                    Console.WriteLine("KqlQuery file doesnt exist: {0}", kqlQueryOption.Value());
                    return -1;
                }

                if (!outputFileOption.HasValue() &&
                !consoleLogOption.HasValue() &&
                !browserLogOption.HasValue())
                {
                    if (!clusterAddressOption.HasValue())
                    {
                        Console.WriteLine("Missing Cluster Address");
                        return -1;
                    }

                    if (!databaseOption.HasValue())
                    {
                        Console.WriteLine("Missing Database Name");
                        return -1;
                    }

                    if (!tableOption.HasValue())
                    {
                        Console.WriteLine("Missing Table Name");
                        return -1;
                    }

                    string authority = "microsoft.com";
                    if (adAuthority.HasValue())
                    {
                        authority = adAuthority.Value();
                    }

                    if (clusterAddressOption.HasValue() && databaseOption.HasValue())
                    {
                        var connectionStrings = GetKustoConnectionStrings(
                            authority,
                            clusterAddressOption.Value(),
                            databaseOption.Value(),
                            adClientAppId.Value(),
                            adKey.Value());

                        kscbIngest = connectionStrings.Item1;
                        kscbAdmin = connectionStrings.Item2;
                    }
                }

                try
                {
                    string Url = "http://localhost:9000/";
                    HttpServer httpServer = null;

                    if (browserLogOption.HasValue())
                    {
                        httpServer = new HttpServer(Url);

                        Console.WriteLine();
                        while (!HttpServer.HasActiveSessions())
                        {
                            Console.Write(".");
                            Thread.Sleep(100);
                        }
                    }

                    if (filterPatternOption.HasValue())
                    {
                        UploadEtlFiles(
                            filterPatternOption.Value(), 
                            kqlQueryOption.Value(), 
                            outputFileOption.Value(),
                            httpServer,
                            kscbAdmin,
                            kscbIngest, 
                            directIngestOption.HasValue(),
                            tableOption.Value(),
                            resetTableOption.HasValue());
                    }
                    else if (sessionOption.HasValue())
                    {
                        UploadRealTime(
                            sessionOption.Value(),
                            kqlQueryOption.Value(),
                            outputFileOption.Value(),
                            httpServer,
                            kscbAdmin,
                            kscbIngest,
                            directIngestOption.HasValue(),
                            tableOption.Value(),
                            resetTableOption.HasValue());
                    }
                    else
                    {
                        Console.WriteLine("Missing required options");
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                return 0; //return 0 on a successful execution
            });
        }

        static void UploadEtlFiles(
            string _filePattern,
            string _queryFile,
            string _outputFileName,
            HttpServer httpServer,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            string[] files;
            if (Path.IsPathRooted(_filePattern))
            {
                // Get directory and file parts of complete relative pattern
                string pattern = Path.GetFileName(_filePattern);
                string relDir = _filePattern.Substring(0, _filePattern.Length - pattern.Length);

                // Get absolute path (root+relative)
                string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string absPath = Path.GetFullPath(Path.Combine(rootDir, relDir));

                // Search files mathing the pattern
                files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
            }
            else
            {
                // input
                string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Get directory and file parts of complete relative pattern
                string pattern = Path.GetFileName(_filePattern);
                string relDir = pattern.Substring(0, _filePattern.Length - pattern.Length);
                // Get absolute path (root+relative)
                string absPath = Path.GetFullPath(Path.Combine(rootDir, relDir));

                // Search files mathing the pattern
                files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
            }

            if (files != null && files.Length > 0)
            {
                var etw = Tx.Windows.EtwTdhObservable.FromFiles(files);
                var ku = CreateUploader(UploadTimespan, httpServer, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
                RunUploader(ku, etw, _queryFile);
            }
        }

        static void UploadRealTime(
            string _sessionName,
            string _queryFile,
            string _outputFileName,
            HttpServer httpServer,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            var etw = Tx.Windows.EtwTdhObservable.FromSession(_sessionName);
            Console.WriteLine();
            Console.WriteLine("Listening to real-time session '{0}'. Press Enter to terminate", _sessionName);

            var ku = CreateUploader(UploadTimespan, httpServer, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
            Task task = Task.Factory.StartNew(() =>
            {
                RunUploader(ku, etw, _queryFile);
            });

            string readline = Console.ReadLine();
            ku.OnCompleted();
        }
    }
}

#endif