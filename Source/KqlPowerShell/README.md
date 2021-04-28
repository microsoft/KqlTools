# Real-Time KQL for PowerShell

To process data with Kusto Query Language (KQL) queries today, users generally have to upload their data to storage first and then query it. The Kql Tools eliminate this need by processing event streams with KQL queries **as events arrive, in real-time.**



## Tracing ETW TCP Events

This walkthrough demonstrates how to use the Real-Time KQL PowerShell Module to explore TCP events emitted by an Event Tracing for Windows (ETW) provider. This walkthrough will use an **Administrator PowerShell** window throughout.



### Start an ETW Trace:

logman is a utility that allows you to start an Event Trace Session for a specific ETW provider or set of providers. Run this command to start an event trace session for the Etw TCP provider:

```
logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p 'Microsoft-Windows-Kernel-Network' 0xffffffffffffffff -ets
```

By running `create trace tcp`, this session has been named "tcp".



### Import RealTimeKql:

```
Import-Module RealTimeKql
```



### Look at raw TCP data:

Run the Get-EtwSession cmdlet to see Etw data. Here "tcp" is the session name used earlier in the logman command to create the event trace session:

```
Get-EtwSession tcp
```



### Simplify TCP event output:

Store this query in a .kql file (for this exercise, we'll assume the file is named **query.kql**):

```
EtwTcp
| where Provider == "Microsoft-Windows-Kernel-Network"
| where EventId in (10, 11)
| extend ProcessName = getprocessname(EventData.PID)
| extend Source = strcat(EventData.saddr, ":", ntohs(EventData.sport))
| extend Destination = strcat(EventData.daddr, ":", ntohs(EventData.dport))
| project Provider, Source, Destination, Opcode, ProcessName
```

You can now use this query to simplify the output you see:

```
Get-EtwSession tcp -Query <path to query.kql> | Format-Table
```



### Aggregate TCP events into 30-second intervals:

To reduce the volume of events printing to your screen, you can apply a different query that aggregates events into 30-second windows. Store this query in a .kql file (for this exercise, we'll assume the file is named **summarize.kql**):

```
 EtwTcp 
| where EventId in (10, 11) 
| extend ProcessName = getprocessname(EventData.PID)
| extend Source = EventData.saddr, SourcePort = ntohs(EventData.sport) 
| extend Destination = EventData.daddr, DestinationPort = ntohs(EventData.dport) 
| extend Size = EventData.size 
| extend ProcessId = EventData.PID 
| summarize Count = count(), Bytes = sum(Size) by bin(TimeCreated, 30s), SourceIpAddress, SourcePort, DestinationIpAddress, DestinationPort, EventId, ProcessId, ProcessName
```

Pass in this new query to the Get-EtwSession cmdlet:

```
Get-EtwSession tcp -Query <path to summarize.kql> | Format-Table
```



### Stop an ETW Trace:

Once you are done exploring events, you can stop the Event Trace Session you started with logman by running the following command:

```
logman.exe stop tcp -ets
```