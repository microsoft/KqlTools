using Kusto.Data;
using Microsoft.Extensions.CommandLineUtils;
using SimpleCsvReader;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RealTimeKql
{
    partial class Program
    {
        public static void InvokeCsv(CommandLineApplication command)
        {
            command.Description = "Realtime filter of Csv files";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter events within a CSV file." + Environment.NewLine
                + Environment.NewLine + "Sample usage"
                + Environment.NewLine + "\tRealTimeKql csv --file=Sample.csv --outputconsole" + Environment.NewLine;

            command.HelpOption("-?|-h|--help");

            // input
            var filePathOption = command.Option("-f|--file <value>",
                "Path to CSV file to process. eg, --file=SampleLogs.csv",
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

                if (filePathOption.HasValue() && !File.Exists(filePathOption.Value()))
                {
                    Console.WriteLine("Csv file specified doesn't exist: {0}", filePathOption.Value());
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
                    ProcessCsvRealTime(
                    filePathOption.Value(),
                    kqlQueryOption.Value(),
                    outputFileOption.Value(),
                    blobStorageConnectionStringOption.Value(),
                    blobStorageContainerOption.Value(),
                    kscbAdmin,
                    kscbIngest,
                    directIngestOption.HasValue(),
                    tableOption.Value(),
                    resetTableOption.HasValue());
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

        private static void ProcessCsvRealTime(
            string fileName, 
            string queryFile,
            string consoleLogOption, 
            string outputFileName, 
            string blobConnectionString, 
            string blobContainerName, 
            KustoConnectionStringBuilder kscbAdmin, 
            KustoConnectionStringBuilder kscbIngest, 
            bool directIngest, 
            string tableName, 
            bool resetTable)
        {
            BlockingKustoUploader ku = null;
            FileOutput fileOutput = null;
            ConsoleOutput consoleOutput = null;

            // initiating simple csv reader
            SimpleCsvParser simpleCsvParser = new SimpleCsvParser(fileName);

            // initiating output
            if (kscbAdmin != null)
            {
                // output to kusto
                ku = CreateUploader(UploadTimespan, blobConnectionString, blobContainerName, kscbAdmin, kscbIngest, directIngest, tableName, resetTable);
                Task task = Task.Factory.StartNew(() =>
                {
                    RunUploader(ku, simpleCsvParser, queryFile);
                });
            }
            else if (!string.IsNullOrEmpty(outputFileName))
            {
                // output to file
                fileOutput = new FileOutput(outputFileName);
                RunFileOutput(fileOutput, simpleCsvParser, queryFile);
            }
            else
            {
                // output to console
                bool tableFormat = consoleLogOption == "table" ? true : false;
                consoleOutput = new ConsoleOutput(tableFormat);
                RunConsoleOutput(consoleOutput, simpleCsvParser, queryFile);
            }

            // starting simple csv reader
            simpleCsvParser.Start();

            string readline = Console.ReadLine();

            // clean up
            simpleCsvParser.Stop();
            if (kscbAdmin != null)
            {
                ku.OnCompleted();
            }
            else if (!string.IsNullOrEmpty(outputFileName))
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
