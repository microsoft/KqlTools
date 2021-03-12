using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Kql.CustomTypes;
using System.Threading;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Newtonsoft.Json.Linq;

namespace RealTimeKqlLibrary
{
    public class AdxOutput : IOutput
    {
        public AutoResetEvent Completed { get; private set; }

        private readonly string _table;
        private readonly bool _resetTable;

        private readonly KustoConnectionStringBuilder kscbAdmin;
        private readonly KustoConnectionStringBuilder kscbIngest;
        private readonly KustoIngestionProperties _ingestionProperties;
        private readonly IKustoIngestClient _ingestClient;

        private readonly object _uploadLock = new object();
        private bool _error = false;
        private string[] _fields;
        private List<IDictionary<string, object>> _nextBatch;
        private List<IDictionary<string, object>> _currentBatch;
        private DateTime _lastUploadTime;
        private readonly TimeSpan _flushDuration;
        private readonly int _batchSize;
        private bool _initializeTable;

        public AdxOutput(
            string authority,
            string appclientId,
            string appKey,
            string cluster,
            string database,
            string table,
            bool resetTable=false,
            bool directIngest=false)
        {
            _table = table;
            _resetTable = resetTable;

            _batchSize = 10000;
            _flushDuration = TimeSpan.FromMilliseconds(5);
            _lastUploadTime = DateTime.UtcNow;
            _initializeTable = false;
            _nextBatch = new List<IDictionary<string, object>>();

            Completed = new AutoResetEvent(false);

            // Setting up kusto connection
            if (!string.IsNullOrEmpty(authority))
            {
                if (!string.IsNullOrEmpty(appclientId) && !string.IsNullOrEmpty(appKey))
                {
                    kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{cluster}", database).WithAadApplicationKeyAuthentication(appclientId, appKey, authority);
                    kscbAdmin = new KustoConnectionStringBuilder($"https://{cluster}", database).WithAadApplicationKeyAuthentication(appclientId, appKey, authority);
                }
#if NET462
                else
                {
                    kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{cluster}", database).WithAadUserPromptAuthentication(authority);
                    kscbAdmin = new KustoConnectionStringBuilder($"https://{cluster}", database).WithAadUserPromptAuthentication(authority);
                }
#endif
            }

            if(kscbAdmin != null)
            {
                _ingestionProperties = new KustoIngestionProperties(kscbIngest.InitialCatalog, table);

                if(directIngest)
                {
                    _ingestClient = KustoIngestFactory.CreateDirectIngestClient(this.kscbAdmin);
                }
                else
                {
                    _ingestClient = KustoIngestFactory.CreateQueuedIngestClient(kscbIngest);
                }
            }
            else
            {
                Console.WriteLine($"ERROR getting ADX connection strings. Please double check the information provided.");
                _error = true;
            }
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_error) return;

            if(_fields == null)
            {
                // discover fields on first event
                _fields = obj.Keys.ToArray();
            }

            if (kscbAdmin != null && _initializeTable == false)
            {
                CreateOrResetTable(obj);
                _initializeTable = true;
            }

            DateTime now = DateTime.UtcNow;
            if (_nextBatch.Count >= _batchSize
                || (_flushDuration != TimeSpan.MaxValue && now > _lastUploadTime + _flushDuration))
            {
                UploadBatch();
            }

            _nextBatch.Add(obj);
        }

        public void OutputError(Exception ex)
        {
            _error = true;
            Console.WriteLine(ex.Message);
        }

        public void OutputCompleted()
        {
            if (!_error)
            {
                UploadBatch();
            }

            Completed.Set();
            Console.WriteLine("\nCompleted!");
            Console.WriteLine("Thank you for using RealTimeKql!");
        }

        public void Stop()
        {
            Completed.WaitOne();
            System.Environment.Exit(0);
        }

        private void UploadBatch()
        {
            lock (_uploadLock)
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
                            Console.Write($"{_currentBatch.Count}, ");
                        }
                    }

                    _currentBatch = null;
                    _lastUploadTime = DateTime.UtcNow;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void CreateOrResetTable(IDictionary<string, object> value)
        {
            using (var admin = KustoClientFactory.CreateCslAdminProvider(kscbAdmin))
            {
                if (_resetTable)
                {
                    string dropTable = CslCommandGenerator.GenerateTableDropCommand(_table, true);
                    admin.ExecuteControlCommand(dropTable);
                }

                CreateMergeKustoTable(admin, value);
            }
        }

        private void CreateMergeKustoTable(ICslAdminProvider admin, IDictionary<string, object> value)
        {
            TableSchema tableSchema = new TableSchema(_table);
            foreach (var pair in value)
            {
                tableSchema.AddColumnIfMissing(new ColumnSchema(pair.Key, _columnType[pair.Value != null ? pair.Value.GetType() : typeof(string)]));
            }

            string createTable = CslCommandGenerator.GenerateTableCreateMergeCommand(tableSchema);
            admin.ExecuteControlCommand(createTable);

            string enableIngestTime = CslCommandGenerator.GenerateIngestionTimePolicyAlterCommand(_table, true);
            admin.ExecuteControlCommand(enableIngestTime);
        }

        private readonly Dictionary<Type, string> _columnType = new Dictionary<Type, string>
        {
            { typeof(string), typeof(string).ToString() },
            { typeof(bool), typeof(bool).ToString() },
            { typeof(ushort), typeof(int).ToString() },
            { typeof(byte), typeof(int).ToString() },
            { typeof(int), typeof(int).ToString() },
            { typeof(uint), typeof(int).ToString() },
            { typeof(long), typeof(long).ToString() },
            { typeof(DateTime), typeof(DateTime).ToString() },
            { typeof(TimeSpan), typeof(TimeSpan).ToString() },
            { typeof(Guid), typeof(Guid).ToString() },
            // Dynamic type
            { typeof(Dictionary<string, object>), typeof(JToken).ToString() },
            { typeof(IDictionary<string, object>), typeof(JToken).ToString() },
            { typeof(JToken), typeof(JToken).ToString() },
            { typeof(JObject), typeof(JToken).ToString() },
            { typeof(object), typeof(JToken).ToString() }
        };
    }
}
