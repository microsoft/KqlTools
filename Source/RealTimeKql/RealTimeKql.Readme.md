# RealtimeKql command line tool

This tool is intended for processing Windows OS logs and ETW logs. For upload to Kusto to work, the tool assumes the current logged on user has table creation privileges in the Azure Data Explorer (Kusto) cluster.

Help output is as follows: 

	>RealTimeKql --help

	Version 1.0.0
	Usage: RealTimeKql.exe [options] [command]

	Options:
  		-?|-h|--help  Show help information
  		-v|--version  Show version information

	Commands:
  		Etw     Realtime filter of ETW Events
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
  		-?|-h|--help                 Show help information
  		-o|--outputfile <value>      Write output to file. eg, --outputfile=FilterOutput.json
  		-c|--clusteraddress <value>  Azure Data Explorer (Kusto) cluster address. eg, --clusteraddress=CDOC.kusto.windows.net
  		-d|--database <value>        Azure Data Explorer (Kusto) database name. eg, --database=TestDb
 		-t|--table <value>           Azure Data Explorer (Kusto) table name. eg, --table=OutputTable
  		-f|--filter <value>          File extension pattern to filter files by. eg, --filter=*.evtx
  		-l|--logname <value>         logName can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security
  		-w|--wecfile <value>         Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events
  		-q|--kqlquery <value>        Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/
  		-qi|--quickingest            Default upload to Kusto is using queued ingest. Use this option to do a direct ingest to Kusto.
  		-r|--resettable              The existing data in the destination table is dropped before new data is logged.
  		-e|--readexisting            By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.
  		-lc|--logtoconsole           Log the output to console.

	Use this option to filter OS or application log you see in EventVwr. This option can also be used with log file(s) on disk. Example is file(s) copied from another machine.

	Real-time session using WecFilter xml
        RealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=EvtxOutput --wecFile=WecFilter.xml --readexisting --quickingest --resettable --kqlquery=QueryFile.csl

	Real-time session using Log
        RealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=AzInfoProtectOutput --logname="Azure Information Protection" --readexisting --quickingest --resettable --kqlquery=QueryFile.csl

	Note: To use real-time mode, the tool must be run with winlog reader permissions

	Previously recorded Evtx Trace Log (.evtx files)
        RealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=SecurityEvtx --filter=*.evtx --kqlquery=ProcessCreation.csl

	When Kusto is not accessible, we can log the data to a text file.
        RealtimeKql winlog --outputfile=AzInfoProtectionLog.json --logname="Azure Information Protection" --readexisting --quickingest --resettable --kqlquery=QueryFile.csl

	WinLogKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx logName:"Security"

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

	RealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=EvtxOutput --wecFile=WecFilter.xml --readexisting --quickingest --resettable --kqlquery=QueryFile.csl


## Pre-processing with Rx.KQL

Typically, you would want to use one of the modes above to upload all the data to Kusto and understand what it means. Then you define Kusto queries, and realize that much of the data is unnecessary.

At that point you can use Rx.KQL on the real-time stream to pre-process the events before uploading:

	RealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=SecurityEvents --logName="Security" --kqlquery=ProcessCreation.csl

Here the pre-processing query is filtering Security Events by EventId 4688 and listing some properties:

	Security
		| where EventId == 4688
		| extend ProcessName = EventData.NewProcessName
		| extend ParentProcessName = EventData.ParentProcessName
		| project TimeCreated, ProcessName, ParentProcessName

