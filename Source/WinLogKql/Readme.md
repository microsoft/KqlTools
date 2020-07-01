# WinLog2Kusto command line tool

This tool is intended for uploading Windows OS logs

### Real time mode

In real-time mode the tool listens to local or remote OS Log and continuously uploads the events. To upload a single log run the following from administrator command prompt :

```
WinLog2Kusto cluster:CDOC database:GeorgiTest table:SecurityEvtx logName:"Security"
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
WinLog2Kusto cluster:CDOC database:GeorgiTest table:SecurityEvtx wecFile:WecFilter.xml 
```

### File upload mode

In this mode you can upload existing EVTX file:
```
WinLog2Kusto cluster:CDOC database:GeorgiTest table:Test file:*.evtx 
```

## Pre-processing with Rx.KQL

Typically, you would want to use one of the modes above to upload all the data to Kusto and understand what it means. Then you define Kusto queries, and realize that much of the data is unnecessary.

At that point you can use Rx.KQL on the real-time stream to pre-process the events before uploading:
```
WinLog2Kusto cluster:CDOC database:GeorgiTest table:EtwTcp logName:"Security" query:ProcessCreation.csl
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