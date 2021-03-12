using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Kql.CustomTypes;
using System.Threading;
using System.Threading.Tasks;

namespace RealTimeKqlLibrary
{
    public class BlobOutput : IOutput
    {
        public AutoResetEvent Completed { get; private set; }

        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        private readonly object _uploadLock = new object();
        private bool _error = false;
        private string[] _fields;
        private List<IDictionary<string, object>> _nextBatch;
        private List<IDictionary<string, object>> _currentBatch;
        private DateTime _lastUploadTime;
        private readonly TimeSpan _flushDuration;
        private readonly int _batchSize;

        public BlobOutput(string connectionString, string containerName)
        {
            _batchSize = 10000;
            _flushDuration = TimeSpan.FromMilliseconds(5);
            _lastUploadTime = DateTime.UtcNow;
            _nextBatch = new List<IDictionary<string, object>>();

            Completed = new AutoResetEvent(false);

            if(!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(containerName))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                _blobContainerClient.CreateIfNotExists();
            }
            else
            {
                Console.WriteLine($"ERROR setting up connection to Blob. Please double check the information provided.");
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

            if (_fields == null)
            {
                // discover fields on first event
                _fields = obj.Keys.ToArray();
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

        public void Stop()
        {
            if (!_error)
            {
                UploadBatch();
            }

            Console.WriteLine("\nCompleted!");
            Console.WriteLine("Thank you for using RealTimeKql!");
            Completed.Set();
        }

        private void UploadBatch()
        {
            lock(_uploadLock)
            {
                if(_currentBatch != null)
                {
                    throw new Exception("Upload must not be called before the batch currently being uploaded is complete");
                }

                _currentBatch = _nextBatch;
                _nextBatch = new List<IDictionary<string, object>>();

                try
                {
                    if (_currentBatch.Count > 0)
                    {
                        //Create a blob with unique names and upload
                        string blobName = $"{Guid.NewGuid()}_1_{Guid.NewGuid():N}.json";
                        var blobClient = _blobContainerClient.GetBlobClient(blobName);
                        UploadToContainerAsync(blobClient, _currentBatch).Wait();
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

        private async Task UploadToContainerAsync(BlobClient blobClient, object obj)
        {
            using (var ms = new MemoryStream())
            {
                var json = JsonConvert.SerializeObject(obj);
                using (StreamWriter writer = new StreamWriter(ms))
                {
                    writer.Write(json);
                    writer.Flush();
                    ms.Position = 0;

                    await blobClient.UploadAsync(ms);

                    var properties = await blobClient.GetPropertiesAsync();
                    BlobHttpHeaders headers = new BlobHttpHeaders
                    {
                        // Set the MIME ContentType every time the properties 
                        // are updated or the field will be cleared
                        ContentType = "application/json",

                        // Populate remaining headers with 
                        // the pre-existing properties
                        CacheControl = properties.Value.CacheControl,
                        ContentDisposition = properties.Value.ContentDisposition,
                        ContentEncoding = properties.Value.ContentEncoding,
                        ContentHash = properties.Value.ContentHash
                    };

                    // Set the blob's properties.
                    await blobClient.SetHttpHeadersAsync(headers);
                }
            }
        }
    }
}
