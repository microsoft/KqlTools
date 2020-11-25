# Query Guide

The query file describes what processing to apply to the events on the stream. It uses a subset of the Kusto Query Language (KQL) which is specifically useful for real-time viewing and prepossessing of streams.

* [Best practice for creating query files](#BestPractice)
* [Examples and use cases](#Examples)
  * [**WinLog:** Process start events only](#WinlogProcessStart)
  * [**Etw:** Tcp packet sent and received events only](#EtwTcp)
  * [**Syslog:** Failed login attempts only](#SyslogFailedLogin)



## <a id="BestPractice"></a>Best Practice for Creating Query Files

You can watch a demonstration of the best practice for creating query files [here](https://youtu.be/GoTSuWPrkig).

**General Guideline:**

1. Upload some raw events into Kusto (ADX) or a local JSON file, without specifying a query file
2. Look at the data and determine which components are interesting to you
3. Define a query that shows what you want as output
4. Save the query as a **.kql** or **.csl** file and pass this in to Real-Time KQL through the command line



## <a id="Examples"></a>Examples and Use Cases

### <a id="WinlogProcessStart"></a>WinLog: Process Start Events

If you are only interested in examining Process Start events on your Windows machine, you can accomplish this easily with Real-Time KQL.

**Query**

```
Security 
| where EventId == 4688
| extend ProcessName = EventData.NewProcessName
| extend ParentProcessName = EventData.ParentProcessName
| project TimeCreated, ProcessName, ParentProcessName
```

**Query Breakdown**

* `Security` : in KQL, this would be the name of the table you wish to apply the query on. For the purposes of Real-Time KQL, you can set this name to be anything
* `| where EventId == 4688` : ignore any events that do not have an `EventId `of `4688` (the event Id for Process Start events)
* `| extend ProcessName = EventData.NewProcessName` : retrieve the process name from the `EventData.NewProcessName `field and store it under a new column by the name of `ProcessName`
* `| extend ParentProcessName = EventData.ParentProcessName` : retrieve the parent process name from `EventData.ParentProcessName` and store that under a new column by the name of `ParentProcessName`
* `| project TimeCreated, ProcessName, ParentProcessName` : display the `TimeCreated`, `ProcessName`, and `ParentProcessName` columns to output

**Example Usage**

`RealTimeKql WinLog --log=Security --query=ProcessCreation.csl --outputconsole`

**Example Usage Breakdown**

* `--log=Security` : attach Real-Time KQL to the `Security` log, where all Process Start events are logged
* `--query=ProcessCreation.csl` : use the query stored in [ProcessCreation.csl](https://github.com/microsoft/KqlTools/blob/master/Source/RealTimeKql/ProcessCreation.csl) to filter events in the `Security` log
* `--outputconsole` : print the results to console



### <a id="EtwTcp"></a>ETW: Tcp Send and Receive Events

**Query**

```
EtwTcp 
| where EventId in (10, 11)
| extend ProcessName = getprocessname(EventData.PID)
| extend SourceIpAddress = strcat(EventData.saddr, ":", ntohs(EventData.sport))
| extend DestinationIpAddress = strcat(EventData.daddr, ":", ntohs(EventData.sport))
| summarize _count = count() by SourceIpAddress, DestinationIpAddress, EventId, ProcessName, bin(TimeCreated, 2m)
```

**Query Breakdown**

* `EtwTcp` : in KQL, this would be the name of the table you wish to apply the query on. For the purposes of Real-Time KQL, you can set this name to be anything
* `| where EventId in (10, 11)` : ignore any events without an `EventId` of 10 or 11 (the event ids for tcp packet sent and received)
* `| extend ProcessName = getprocessname(EventData.PID)` : retrieve the process name using the `getprocessname()` enrichment function and store it in a new column named `ProcessName`
* `| extend SourceIpAddress = strcat(EventData.saddr, ":", ntohs(EventData.sport))` : retrieve the source IP address and port from `Event Data` and store it in a new column named `SourceIpAddress`
*  `| extend DestinationIpAddress = strcat(EventData.daddr, ":", ntohs(EventData.sport))` : retrieve the destination IP address and port from `EventData` and store it in a new column named `DestinationIpAddress`
* `| summarize _count = count() by SourceIpAddress, DestinationIpAddress, EventId, ProcessName, bin(TimeCreated, 2m)` : aggregate logs by the columns specified into 2-minute time windows (indicated by `bin(TimeCreated, 2m)`). The number of logs aggregated will be stored in the `_count` column

**Example Usage**

`RealTimeKql etw --session=tcp --query=Summarize.csl --outputjson=Tcp.json`

**Example Usage Breakdown**

* `--session=tcp` : the name of the etw session to attach Real-Time KQL to
* `--query=Summarize.csl` : use the query stored in [Summarize.csl](https://github.com/microsoft/KqlTools/blob/master/Source/RealTimeKql/SummarizeQuery.csl) to filter events
* `--outputjson=Tcp.json` : write the results to `Tcp.json`



### <a id="SyslogFailedLogin"></a>Syslog: Failed Login Events

You can use Real-Time KQL to transform and filter syslog data in a way that makes it easy to understand what's happening on your machine. This example allows you, with just a quick glance at your screen, to see how many times a failed login attempt has been made under your username.

**Query**

```
Extract
| extend User = extract("((Failed|Accepted)\\s\\w*)sfor\\s(\\w*)", 3, Payload)
| extend Activity = extract("((Failed|Accepted)\\s\\w*)\\sfor\\s(\\w*)", 1, Payload)
| where isnotnull(Activity) and isnotnull(User)
| where Activity has "Failed"
| project User, Activity
```

**Query Breakdown**

* `Extract` : in KQL, this would be the name of the table you wish to apply the query on. For the purposes of Real-Time KQL, you can set this name to be anything
* `| extend User = extract("((Failed|Accepted)\\s\\w*)sfor\\s(\\w*)", 3, Payload)` : create a column named `User` and use the Regex expression in `extract()` to parse the `Payload` field for the name of the user
* `| extend Activity = extract("((Failed|Accepted)\\s\\w*)\\sfor\\s(\\w*)", 1, Payload)` : create a column named `Activity` and use the Regex expression in `extract()` to parse the `Payload` field for the type of activity that has occurred
* `| where isnotnull(Activity) and isnotnull(User)` : ignore any log entries where `User` or `Activity` is null
* `| where Activity has "Failed"` : ignore any log entries where the `Activity` is anything except a failed login attempt
* `| project User, Activity` : display the `User` and `Activity` columns to output

**Example Usage**

`sudo ./RealTimeKql syslog --logfile=/var/log/auth.log --query=FailedLogins.csl --outputconsole `

**Example Usage Breakdown**

* `--logfile=/var/log/auth.log` : attach Real-Time KQL to the `/var/log/auth.log` file
* `--query=FailedLogins.csl` : use the query stored in `FailedLogins.csl` to filter events
* `--outputconsole` : write the results to console