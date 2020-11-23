# WinLog

This input option allows you to filter the OS or application logs you see in EventVwr. It can also be used with log file(s) on disk (e.g. file(s) copied from another machine). 

**You can watch a demonstration of using Real-Time KQL for WinLog [here](https://youtu.be/GoTSuWPrkig).**

**Jump To:**

* [Real-Time Monitoring](#RealTimeMonitoring)
  * [Using a WEC File](#UsingAWecFile)
  * [Using Log](#UsingLog) 
* [Historical Processing](#HistoricalProcessing)
  * [Using a previously recorded Evtx Trace Log (.evtx files)](#RecordedEvtx)
* [WinLog Options Overview](#WinLogOptionsOverview)



## <a id="RealTimeMonitoring"></a>Real-Time Monitoring

### <a id="UsingAWecFile"></a>Using a WEC File

You can use an XML-formatted query file to filter through windows event logs.

**Example usage**:

`RealTimeKql WinLog --wecfile=WecFilter.xml --readexisting --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset`

**Example breakdown**:

* `--wecfile=WecFilter.xml` : use the XML-formatted query file [WecFilter.xml](https://github.com/microsoft/KqlTools/blob/master/Source/RealTimeKql/WecFilter.xml) to filter through windows logs
*  `--readexisting` : start reading events from the beginning of the log (as opposed to just future logs)
* `--adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput` : ingest all results to the "EvtxOutput" table in the "GeorgiTest" database in the "CDOC.kusto.windows.net" Azure Data Explorer (ADX) cluster
* `--adxdirect` : use [direct ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#direct-ingestion) instead of the default [queued ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#queued-ingestion)
* `--adxreset` : if the "EvtxOutput" table already exists, reset it

### <a id="UsingLog"></a>Using Log

You can also simply specify the specific log you wish to monitor and use Real-Time KQL to process events generated in that log.

**Example usage:**

`RealtimeKql winlog --log="Security" --readexisting --query=ProcessCreation.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityOutput --adxdirect --adxreset`

**Example breakdown:**

* `--log="Security"` : monitor the Security log
*  `--readexisting` : start reading events from the beginning of the log (as opposed to just future logs)
*  `--query=ProcessCreation.csl` : use [ProcessCreation.csl](../Source/RealTimeKql/ProcessCreation.csl) to process all events (in this case, ignore all events that are not process creation events). For more information on creating and using queries, see the [query writing guide](QueryGuide.md)
* `--adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityOutput` : ingest all results to the "SecurityOutput" table in the "GeorgiTest" database in the "CDOC.kusto.windows.net" Azure Data Explorer (ADX) cluster
* `--adxdirect` : use [direct ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#direct-ingestion) instead of the default [queued ingestion](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/netfx/about-kusto-ingest#queued-ingestion)
* `--adxreset` : if the "SecurityOutput" table already exists, reset it



## <a id="HistoricalProcessing"></a>Historical Processing

### <a id="RecordedEvtx"></a>Previously Recorded Evtx Trace Log (.evtx files)

You can also use Real-Time KQL to process pre-recorded Evtx Trace log files (*.evtx files).

**Example usage**:

`RealtimeKql winlog --file=*.evtx --query=ProcessCreation.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityEvtx`

**Example breakdown**:

* `--file=*.evtx` : use the pattern "*.evtx" to filter for files of interest
* `--query=ProcessCreation.csl`: use [ProcessCreation.csl](../Source/RealTimeKql/ProcessCreation.csl) to process all events (in this case, ignore all events that are not process creation events). For more information on creating and using queries, see the [query writing guide](QueryGuide.md)
* `--adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityEvtx` : ingest all results to the "SecurityEvtx" table in the "GeorgiTest" database in the "CDOC.kusto.windows.net" Azure Data Explorer (ADX) cluster



## <a id="WinLogOptionsOverview"></a>WinLog Options Overview

You can also run`RealTimeKql Winlog --help ` from an Administrator Command Prompt to get this same overview of your options:

```
Usage: RealTimeKql.exe WinLog [options]

Options:
  -?|-h|--help                                 Show help information
  -l|--log <value>                             log can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security
  -e|--readexisting                            By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.
  -w|--wecfile <value>                         Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events
  -f|--file <value>                            File pattern to filter files by. eg, --file=*.evtx
  -q|--query <value>                           Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/
  -oc|--outputconsole                          Log the output to console.
  -oj|--outputjson <value>                     Write output to JSON file. eg, --outputjson=FilterOutput.json
  -bscs|--blobstorageconnectionstring <value>  Azure Blob Storage Connection string. Optional when want to upload as JSON to blob storage.
  -bsc|--blobstoragecontainer <value>          Azure Blob Storage container name. Optional when want to upload as JSON to blob storage.
  -ad|--adxauthority <value>                   Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com
  -aclid|--adxclientid <value>                 Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.
  -akey|--adxkey <value>                       Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id
  -ac|--adxcluster <value>                     Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net
  -ad|--adxdatabase <value>                    Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb
  -at|--adxtable <value>                       Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable
  -ar|--adxreset                               The existing data in the destination table is dropped before new data is logged.
  -ad|--adxdirect                              Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.
```