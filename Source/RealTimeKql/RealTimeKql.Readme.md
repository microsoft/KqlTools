# RealtimeKql command line tool

This tool is intended for processing Windows OS logs, ETW logs and Syslog. For upload to Kusto to work, the tool assumes the current logged on user has table creation privileges in the Azure Data Explorer (Kusto) cluster.

Help output is as follows: 

	>RealTimeKql --help

	Version 1.0.0
	Usage: RealTimeKql.exe [options] [command]

	Options:
  		-?|-h|--help  Show help information
  		-v|--version  Show version information

	Commands:
		Etw     Realtime filter of ETW Events
		Syslog  Realtime processing of Syslog Events
		WinLog  Realtime filter of Winlog Events

	Use "RealTimeKql.exe [command] --help" for more information about a command.

	RealTimeKql.exe allows user to filter the stream and show only the events of interest.
	Kusto Query language is used for defining the queries.
	Learn more about the query syntax at, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/

	All values must follow the parameter with an equals sign (=), or the key must have a prefix (-- or /) when the value follows a space. The value isn't required if an equals sign is used (for example, CommandLineKey=).

### Realtime Kql for Winlog

In real-time mode the tool listens to local or remote OS Log and continuously uploads the events. To upload a single log run the following from administrator command prompt :

	Usage: RealTimeKql.exe WinLog [options]

	Options:
		-?|-h|--help               Show help information
		-l|--log <value>           log can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security
		-e|--readexisting          By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.
		-w|--wecfile <value>       Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events
		-f|--file <value>          File pattern to filter files by. eg, --file=*.evtx
		-q|--query <value>         Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/
		-oc|--outputconsole        Log the output to console.
		-oj|--outputjson <value>   Write output to JSON file. eg, --outputjson=FilterOutput.json
		-ad|--adxauthority <value>    Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com
		-aclid|--adxclientid <value>  Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.
		-akey|--adxkey <value>        Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id
		-ac|--adxcluster <value>   Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net
		-ad|--adxdatabase <value>  Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb
		-at|--adxtable <value>     Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable
		-ar|--adxreset             The existing data in the destination table is dropped before new data is logged.
		-ad|--adxdirect            Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.

	Use this option to filter OS or application log you see in EventVwr. This option can also be used with log file(s) on disk. Example is file(s) copied from another machine.

	Real-time session using WecFilter xml
        RealtimeKql winlog --wecfile=WecFilter.xml --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset

	Real-time session using Log
        RealtimeKql winlog --log="Azure Information Protection" --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=AzInfoProtectOutput --adxdirect --adxreset

	Note: To use real-time mode, the tool must be run with winlog reader permissions

	Previously recorded Evtx Trace Log (.evtx files)
        RealtimeKql winlog --file=*.evtx --query=ProcessCreation.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityEvtx

	When Kusto is not accessible, we can log the data to a text file.
        RealtimeKql winlog --log="Azure Information Protection" --readexisting --query=QueryFile.csl --outputjson=AzInfoProtectionLog.json

The tool can also upload selected set of logs, filtered by provider ID,  event IDs and everything else that can be described with XPath. To do this, the set of XPath queries can be described in standard XML file:

	<QueryList>
		<Query Id="0" Path="System">
	    	<Select Path="System">*[System[Provider[@Name='Service Control Manager'] and (EventID=7000 or EventID=7040 or EventID=7045)]]</
	   		<Select Path="System">*[System[Provider[@Name='Microsoft-Windows-Kernel-General'] and (EventID=1 or EventID=12 or EventID=13)]]</Select>
	     </Query>
		 <Query Id="1" Path="Application">
			...
		 </Query>
	</QueryList>


This file can be passed from the command line as follows:

	RealtimeKql winlog --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --wecfile=WecFilter.xml --readexisting --adxdirect --adxreset --query=QueryFile.csl

## Pre-processing WinLog with Rx.KQL

Typically, you would want to use one of the modes above to upload all the data to Kusto and understand what it means. Then you define Kusto queries, and realize that much of the data is unnecessary.

At that point you can use Rx.KQL on the real-time stream to pre-process the events before uploading:

	RealtimeKql winlog --log="Security" --readexisting --query=QueryFile.csl --outputjson=Security.json

Here the pre-processing query is filtering Security Events by EventId 4688 and listing some properties:

	Security
		| where EventId == 4688
		| extend ProcessName = EventData.NewProcessName
		| extend ParentProcessName = EventData.ParentProcessName
		| project TimeCreated, ProcessName, ParentProcessName

## Realtime Kql for Etw
The user can start an Etw session and then get RealtimeKql tool attach to that Etw session and process the events that it receives.

	Usage: RealTimeKql Etw [options]

	Options:
		-?|-h|--help               Show help information
		-s|--session <value>       Name of the ETW Session to attach to. eg, --session=tcp. tcp is the name of the session started using logman or such tools.
		-f|--file <value>          File pattern to filter files by. eg, --filter=*.etl
		-q|--query <value>         Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/
		-oc|--outputconsole        Log the output to console.
		-oj|--outputjson <value>   Write output to JSON file. eg, --outputjson=FilterOutput.json
		-ad|--adxauthority <value>    Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com
		-aclid|--adxclientid <value>  Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.
		-akey|--adxkey <value>        Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id
		-ac|--adxcluster <value>   Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net
		-ad|--adxdatabase <value>  Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb
		-at|--adxtable <value>     Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable
		-ar|--adxreset             The existing data in the destination table is dropped before new data is logged.
		-ad|--adxdirect            Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.

	Use this option to filter ETW events that are logged to the trace session. This option can also be used with ETL log file(s) on disk. Example is file(s) copied from another machine or previous ETW sessions.

	Real-time session
        RealtimeKql etw --session=tcp --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp --adxdirect --adxreset

	Note: To use real-time mode, the tool must be run with ETW reader permissions

	Previously recorded ETL Trace Log (.etl files)
        RealtimeKql etw --filter=*.etl --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp

	When Kusto is not accessible, we can log the data to a text file.
        RealtimeKql etw --session=tcp --query=QueryFile.csl --outputjson=Tcp.json

	Note: Logman can be used to start a ETW trace. In this example we are creating a trace session named tcp with Tcp Provider guid.

        logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets

	When done, stopping the trace session is using command,
        logman.exe stop tcp -ets

	If you are getting the error Unexpected TDH status 1168, this indicates ERROR_NOT_FOUND.
	Only users with administrative privileges, users in the Performance Log Users group, and applications running as LocalSystem, LocalService, NetworkService could do that. It could mean that you are running the application as someone who doesnt belong to the above mentioned user groups.To grant a restricted user the ability to consume events in real time, add them to the Performance Log Users group.

## Pre-processing ETW with Rx.KQL

In this example lets create a query file that contains a query like the one here and pass it as input to the tool, 

    RealtimeKql etw --session=tcp --query=QueryFile.csl --outputjson=Tcp.json

	EtwTcp 
	  | where EventId in (10, 11)
	  | extend ProcessName = getprocessname(EventData.PID)
	  | extend SourceIpAddress = strcat(EventData.saddr, ":", ntohs(EventData.sport))
	  | extend DestinationIpAddress = strcat(EventData.daddr, ":", ntohs(EventData.sport))
	  | summarize _count = count() by SourceIpAddress, DestinationIpAddress, EventId, ProcessName, bin(TimeCreated, 2m)

Here, RealtimeKql collects two noisy TCP events 10 and 11 which are DataSent and DataReceived events, summarizes the data into 2m buckets and uploads them. 

## Realtime Kql for Syslog

	Usage: RealTimeKql Syslog [options]

	Options:
		-?|-h|--help                 Show help information
		-n|--networkAdapter <value>  Optional: Network Adapter Name. When not specified, listner listens on all adapters.
		-p|--udpport <value>         Optional: UDP Port to listen on. When not specified listner is listening on port 514.
		-q|--query <value>           Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/
		-oc|--outputconsole          Log the output to console.
		-oj|--outputjson <value>     Write output to JSON file. eg, --outputjson=FilterOutput.json
		-ad|--adxauthority <value>   Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com
		-aclid|--adxclientid <value> Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.
		-akey|--adxkey <value>       Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id
		-ac|--adxcluster <value>     Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net
		-ad|--adxdatabase <value>    Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb
		-at|--adxtable <value>       Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable
		-ar|--adxreset               The existing data in the destination table is dropped before new data is logged.
		-ad|--adxdirect              Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.

	Use this option to listen to Syslog Events.

	Real-time SysLog Events
        RealtimeKql syslog --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset
