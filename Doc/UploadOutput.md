# Upload Output

Real-Time KQL supports uploading output to external storage. The output is treated as a stream and can be infinite.

**Jump To:**

* [Azure Data Explorer Output](#ADXOutput)
* [Blob Storage](#BlobStorage)

## <a id="ADXOutput"></a>Azure Data Explorer Output

With the Azure Data Explorer (Kusto) output option, you can upload events to Kusto for further analysis and querying.

**Example usage - Processing a previously record ETL Trace Log (.etl files):**

`RealtimeKql etw --file=*.etl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp --adxdirect --adxreset`

**Example breakdown**:

* `--file=*.etl` : file pattern to filter files by (in this case, only look for files that match the "*.etl" pattern)
* `--adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp` : ingest all results to the "EtwTcp" table in the "GeorgiTest" database in the "CDOC.kusto.windows.net" Azure Data Explorer (ADX) cluster
* `--adxdirect` : use [direct ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#direct-ingestion) instead of the default [queued ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#queued-ingestion)
* `--adxreset` : if the "EtwTcp" table already exists, reset it



## <a id="BlobStorage"></a> Blob Storage

With the Blob Storage output option, you can upload events as JSON objects to a blob storage.

**Example usage - Monitoring the Windows Application log:**

`RealTimeKql Winlog --log="Application" --blobstorageconnectionstring=connectionstring --blobstoragecontainer=containername`

**Example breakdown:**

* `--log="Application"` : monitor the Application log
* `--blobstorageconnectionstring=connectionstring` : substitute `connectionstring` with your Azure Blob Storage connection string
* `--blobstoragecontainer=containername` : substitute `containername` with your Azure Blob Storage container name