// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

//#define BUILT_FOR_WINDOWS Uncomment this line to get intellisense to work

#if BUILT_FOR_WINDOWS

namespace RealTimeKql
{
    using Kusto.Data;
    using Microsoft.EvtxEventXmlScrubber;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    partial class Program
    {
        public static void InvokeWinLog(CommandLineApplication command)
        {
            command.Description = "Realtime filter of Winlog Events";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter OS or application log you see in EventVwr. This option can also be used with log file(s) on disk. Example is file(s) copied from another machine." + Environment.NewLine
                + Environment.NewLine + "Real-time session using WecFilter xml"
                + Environment.NewLine + "\tRealtimeKql winlog --wecfile=WecFilter.xml --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Real-time session using Log"
                + Environment.NewLine + "\tRealtimeKql winlog --log=\"Azure Information Protection\" --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=AzInfoProtectOutput --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with winlog reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded Evtx Trace Log (.evtx files)"
                + Environment.NewLine + "\tRealtimeKql winlog --file=*.evtx --query=ProcessCreation.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityEvtx" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql winlog --log=\"Azure Information Protection\" --readexisting --query=QueryFile.csl --outputjson=AzInfoProtectionLog.json";

            command.HelpOption("-?|-h|--help");

            // input
            var lognameOption = command.Option("-l|--log <value>",
                "log can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security",
                CommandOptionType.SingleValue);

            var readExistingOption = command.Option("-e|--readexisting",
                "By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.",
                CommandOptionType.NoValue);

            var wecFileOption = command.Option("-w|--wecfile <value>",
                "Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events",
                CommandOptionType.SingleValue);

            var filePatternOption = command.Option("-f|--file <value>",
                "File pattern to filter files by. eg, --file=*.evtx",
                CommandOptionType.SingleValue);

            // query for real-time view or pre-processing
            var kqlQueryOption = command.Option("-q|--query <value>",
                "Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/",
                CommandOptionType.SingleValue);

            // output
            var consoleLogOption = command.Option("-oc|--outputconsole",
                "Optional: Specify the format for console output. eg, --outputconsole=table. The default format for console output is JSON.",
                CommandOptionType.SingleValue);

            var outputFileOption = command.Option("-oj|--outputjson <value>",
                "Write output to JSON file. eg, --outputjson=FilterOutput.json",
                CommandOptionType.SingleValue);

            var blobStorageConnectionStringOption = command.Option("-bscs|--blobstorageconnectionstring <value>",
                "Azure Blob Storage Connection string. Optional when want to upload as JSON to blob storage.",
                CommandOptionType.SingleValue);

            var blobStorageContainerOption = command.Option("-bsc|--blobstoragecontainer <value>",
                "Azure Blob Storage container name. Optional when want to upload as JSON to blob storage.",
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

                if (wecFileOption.HasValue() && !File.Exists(wecFileOption.Value()))
                {
                    Console.WriteLine("Wec file doesnt exist: {0}", wecFileOption.Value());
                    return -1;
                }

                if (kqlQueryOption.HasValue() && !File.Exists(kqlQueryOption.Value()))
                {
                    Console.WriteLine("KqlQuery file doesnt exist: {0}", kqlQueryOption.Value());
                    return -1;
                }

                if (blobStorageConnectionStringOption.HasValue()) //Blob Storage Upload
                {
                    if (!blobStorageContainerOption.HasValue())
                    {
                        Console.WriteLine("Missing Blob Storage Container Name");
                        return -1;
                    }
                }

                if (clusterAddressOption.HasValue() || databaseOption.HasValue() || tableOption.HasValue())
                {
                    // Kusto Upload
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
                    if (filePatternOption.HasValue())
                    {
                        UploadEvtxFiles(
                            filePatternOption.Value(), 
                            kqlQueryOption.Value(),
                            consoleLogOption.Value(),
                            outputFileOption.Value(),
                            blobStorageConnectionStringOption.Value(), 
                            blobStorageContainerOption.Value(),
                            kscbAdmin, 
                            kscbIngest, 
                            directIngestOption.HasValue(), 
                            tableOption.Value(), 
                            resetTableOption.HasValue());
                    }
                    else if (wecFileOption.HasValue() || lognameOption.HasValue())
                    {
                        UploadUsingWecFile(
                            wecFileOption.Value(), 
                            lognameOption.Value(), 
                            kqlQueryOption.Value(), 
                            readExistingOption.HasValue(),
                            consoleLogOption.Value(),
                            outputFileOption.Value(),
                            blobStorageConnectionStringOption.Value(),
                            blobStorageContainerOption.Value(),
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

                return 0;
            });
        }

        private static void UploadEvtxFiles(
            string _filePattern,
            string _queryFile,
            string consoleLogOption,
            string _outputFileName,
            string blobConnectionString,
            string blobContainerName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            string[] files;
            if (Path.IsPathRooted(_filePattern))
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(_filePattern));
                string pattern = Path.GetFileName(_filePattern);
                files = Directory.GetFiles(dir, pattern);
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
                var etw = EvtxAsDictionaryObservable.FromFiles(files);
                if(kscbAdmin != null)
                {
                    // output to kusto  
                    var ku = CreateUploader(UploadTimespan, blobConnectionString, blobContainerName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
                    RunUploader(ku, etw, _queryFile);
                }
                else if (!string.IsNullOrEmpty(_outputFileName))
                {
                    // output to file
                    var fileOutput = new FileOutput(_outputFileName);
                    RunFileOutput(fileOutput, etw, _queryFile);
                }
                else
                {
                    // output to console
                    bool tableFormat = consoleLogOption == "table" ? true : false;
                    var consoleOutput = new ConsoleOutput(tableFormat);
                    RunConsoleOutput(consoleOutput, etw, _queryFile);
                }
            }
        }

        private static void UploadUsingWecFile(
            string _wecFile,
            string _logName,
            string _queryFile,
            bool _readExisting,
            string consoleLogOption,
            string _outputFileName,
            string blobConnectionString,
            string blobContainerName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            IObservable<IDictionary<string, object>> etw;
            BlockingKustoUploader ku = null;
            FileOutput fileOutput = null;
            ConsoleOutput consoleOutput = null;

            if (!string.IsNullOrEmpty(_wecFile))
            {
                if (!File.Exists(_wecFile))
                {
                    Console.WriteLine("Wec File doesnt exist!");
                    return;
                }

                string _wecFileContent = File.ReadAllText(_wecFile);
                etw = Tx.Windows.EvtxObservable.FromLog(_logName, _wecFileContent, _readExisting, null).Select(x => x.Deserialize());

                Console.WriteLine();
                Console.WriteLine("Listening using WecFile '{0}'. Press Enter to terminate", _wecFile);
            }
            else
            {
                etw = Tx.Windows.EvtxObservable.FromLog(_logName, null, _readExisting).Select(x => x.Deserialize());

                Console.WriteLine();
                Console.WriteLine("Listening using Log Name '{0}'. Press Enter to terminate", _logName);
            }

            // output
            if (kscbAdmin != null)
            {
                // output to kusto
                ku = CreateUploader(UploadTimespan, blobConnectionString, blobContainerName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
                Task task = Task.Factory.StartNew(() =>
                {
                    RunUploader(ku, etw, _queryFile);
                });
            }
            else if (!string.IsNullOrEmpty(_outputFileName))
            {
                // output to file
                fileOutput = new FileOutput(_outputFileName);
                RunFileOutput(fileOutput, etw, _queryFile);
            }
            else
            {
                // output to console
                bool tableFormat = consoleLogOption == "table" ? true : false;
                consoleOutput = new ConsoleOutput(tableFormat);
                RunConsoleOutput(consoleOutput, etw, _queryFile);
            }

            string readline = Console.ReadLine();

            // clean up
            if (kscbAdmin != null)
            {
                ku.OnCompleted();
            }
            else if (!string.IsNullOrEmpty(_outputFileName))
            {
                fileOutput.OnCompleted();
            }
            else
            {
                consoleOutput.OnCompleted();
            }
        }
    }
}

#endif