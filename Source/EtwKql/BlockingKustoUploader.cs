// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace EtwKql
{
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Kusto.Ingest;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Kql;
    using System.Threading;

    class BlockingKustoUploader : IObserver<IDictionary<string, object>>
    {
        public string OutputFileName { get; private set; }

        public string TableName { get; private set; }

        public int BatchSize { get; private set; }

        public AutoResetEvent Completed { get; private set; }

        private KustoConnectionStringBuilder Csb { get; set; }

        private KustoConnectionStringBuilder AdminCsb { get; set; }

        private readonly object uploadLock = new object();

        private readonly KustoIngestionProperties _ingestionProperties;

        private IKustoIngestClient _ingestClient;

        private StreamWriter outputFile;

        private string[] _fields;
        private List<IDictionary<string, object>> _nextBatch;
        private List<IDictionary<string, object>> _currentBatch;
        private DateTime _lastUploadTime;
        private TimeSpan _flushDuration;
        private readonly bool _resetTable;
        private bool _initializeTable;

        public BlockingKustoUploader(
            string outputFileName,
            KustoConnectionStringBuilder adminCsb,
            KustoConnectionStringBuilder kscb,
            bool demoMode,
            string tableName,
            int batchSize,
            TimeSpan flushDuration,
            bool resetTable = false)
        {
            OutputFileName = outputFileName;
            Csb = kscb;
            AdminCsb = adminCsb;
            TableName = tableName;
            BatchSize = batchSize;
            _flushDuration = flushDuration;
            _lastUploadTime = DateTime.UtcNow;
            _resetTable = resetTable;
            _initializeTable = false;

            Completed = new AutoResetEvent(false);


            if (Csb != null)
            {
                _ingestionProperties = new KustoIngestionProperties(Csb.InitialCatalog, tableName);

                if (demoMode)
                {
                    _ingestClient = KustoIngestFactory.CreateDirectIngestClient(this.AdminCsb);
                }
                else
                {
                    _ingestClient = KustoIngestFactory.CreateQueuedIngestClient(Csb);
                }
            }

            if (!string.IsNullOrEmpty(OutputFileName))
            {
                outputFile = new StreamWriter(this.OutputFileName);
                outputFile.Write($"[{Environment.NewLine}");
            }

            _nextBatch = new List<IDictionary<string, object>>();
        }

        public void UploadBatch(bool lastBatch)
        {
            lock (uploadLock)
            {
                if (_currentBatch != null)
                {
                    throw new Exception("Upload must not be called before the batch currently being uploaded is complete");
                }

                _currentBatch = _nextBatch;
                _nextBatch = new List<IDictionary<string, object>>();

                try
                {
                    if (_currentBatch.Count > 0)
                    {
                        if (_ingestClient != null)
                        {
                            var data = new DictionaryDataReader(_currentBatch);
                            _ingestClient.IngestFromDataReader(data, _ingestionProperties);
                        }
                        else
                        {
                            string content = lastBatch ?
                                $"{JsonConvert.SerializeObject(_currentBatch, Formatting.Indented).Trim(new char[] { '[', ']' })}" :
                                $"{JsonConvert.SerializeObject(_currentBatch, Formatting.Indented).Trim(new char[] { '[', ']' })},";

                            outputFile.Write(content);
                        }
                    }

                    int recordsUploaded = _currentBatch.Count;
                    _currentBatch = null;
                    _lastUploadTime = DateTime.UtcNow;

                    Console.Write("{0} ", recordsUploaded);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void OnNext(IDictionary<string, object> value)
        {
            if (_fields == null)
            {
                _fields = value.Keys.ToArray();
            }

            if (AdminCsb != null &&
                _initializeTable == false)
            {
                CreateOrResetTable(value);
                _initializeTable = true;
            }


            DateTime now = DateTime.UtcNow;
            if (_nextBatch.Count >= BatchSize
                || (_flushDuration != TimeSpan.MaxValue && now > _lastUploadTime + _flushDuration))
            {
                UploadBatch(false);
            }

            _nextBatch.Add(value);
        }

        public void OnError(Exception error)
        {
            RxKqlEventSource.Log.LogException(error.ToString());
            throw error;
        }

        public void OnCompleted()
        {
            UploadBatch(true);
            outputFile.Write($"]{Environment.NewLine}");
            outputFile.Dispose();
            Console.WriteLine("Completed!");
            Completed.Set();
        }

        private void CreateOrResetTable(IDictionary<string, object> value)
        {
            using (var admin = KustoClientFactory.CreateCslAdminProvider(AdminCsb))
            {
                if (_resetTable)
                {
                    string dropTable = CslCommandGenerator.GenerateTableDropCommand(TableName, true);
                    admin.ExecuteControlCommand(dropTable);
                }

                CreateMergeKustoTable(admin, value);
            }
        }

        private void CreateMergeKustoTable(ICslAdminProvider admin, IDictionary<string, object> value)
        {
            TableSchema tableSchema = new TableSchema(TableName);
            foreach (var pair in value)
            {
                tableSchema.AddColumnIfMissing(new ColumnSchema(pair.Key, _columnType[pair.Value != null ? pair.Value.GetType() : typeof(string)]));
            }

            string createTable = CslCommandGenerator.GenerateTableCreateMergeCommand(tableSchema);
            admin.ExecuteControlCommand(createTable);

            string enableIngestTime = CslCommandGenerator.GenerateIngestionTimePolicyAlterCommand(TableName, true);
            admin.ExecuteControlCommand(enableIngestTime);
        }

        private readonly Dictionary<Type, string> _columnType = new Dictionary<Type, string>
        {
            {typeof(string), typeof(string).ToString() },
            {typeof(ushort), typeof(int).ToString() },
            {typeof(byte), typeof(int).ToString() },
            {typeof(int), typeof(int).ToString() },
            {typeof(uint), typeof(int).ToString() },
            {typeof(long), typeof(long).ToString() },
            {typeof(DateTime), typeof(DateTime).ToString() },
            {typeof(Guid), typeof(Guid).ToString() },
            {typeof(Dictionary<string, object>), typeof(Newtonsoft.Json.Linq.JToken).ToString() }
        };
    }
}