# WinLogKql command line tool

This tool is intended for uploading Windows OS logs. For upload to Kusto to work, the tool assumes the current logged on user has table creation priviliges in the Azure Data Explorer (Kusto) cluster.

### Real time mode

In real-time mode the tool listens to local or remote OS Log and continuously uploads the events. To upload a single log run the following from administrator command prompt :

```
WinLogKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx logName:"Security"
```

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

This file can be passed from the command line as follows
```
WinLogKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx wecFile:WecFilter.xml 
```

### File upload mode

In this mode you can upload existing EVTX file:
```
WinLogKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:Test file:*.evtx 
```

### Output results to local JSON file

In situations where you cannot connect to Azure, the data can be redirected to local JSON.

    WinLogKql outputfile:SecurityEvents.json logName:"Security"

## Pre-processing with Rx.KQL

Typically, you would want to use one of the modes above to upload all the data to Kusto and understand what it means. Then you define Kusto queries, and realize that much of the data is unnecessary.

At that point you can use Rx.KQL on the real-time stream to pre-process the events before uploading:
```
WinLogKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvents logName:"Security" query:ProcessCreation.csl
```

Here the pre-processing query is filtering Security Events by EventId 4688 and listing some properties:

```code
Security 
| where EventId == 4688
| extend ProcessName = EventData.NewProcessName
| extend ParentProcessName = EventData.ParentProcessName
| project TimeCreated, ProcessName, ParentProcessName
```

The pre-processing option is also available with the file-upload mode.