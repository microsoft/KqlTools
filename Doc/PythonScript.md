# Real-Time KQL for Python

Real-Time KQL for Python can run as a regular command-line tool or be called interactively from within the Python shell.

## Contents

* [Downloads](#Downloads)
* [Using Real-Time KQL in Interactive Mode](#InteractiveMode)
  * [Etw TCP Event Tracing](#Etw)
  * [Local Syslog Event Tracing](#Syslog)
* [Other Uses of Real-Time KQL for Python](#OtherUses)
  * [Command line tool](#CommandLineTool)
  * [Direct component use](#DirectComponentUse)



## <a id="Downloads">Downloads

| Windows |  Linux  |
| :-----: | :-----: |
| [x64]() | [x64]() |



## <a id="InteractiveMode">Using Real-Time KQL in Interactive Mode

### <a id="Etw">Etw TCP Event Tracing

The following is a walkthrough example of how to use Real-Time KQL in interactive mode to examine and filter real-time TCP data on your machine:

#### Start an ETW Trace:

In an Administrator command prompt, start an ETW trace session for the TCP provider using logman:

```
logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets
```

#### Examine Raw TCP Events:

From within that same Administrator command prompt, **start a python shell**, and run the following command to import the Real-Time KQL functionality into your shell:

```
from realtimekql import *
```

Then run:

```
run()
```

The script will then begin to prompt you with questions in order to build your event pipeline. Answer the prompts like so:

```
What input source would you like to use? i.e. 'etw'
etw

Please specify the argument associated with this input. i.e. the etw session name
tcp

Would you like to apply a query?
no

What type of output would you like to use? i.e. 'table'
json
```

You can terminate the program at any time by pressing **Ctrl + C**.

#### Simplify TCP Event Output:

Continuing within the same python shell, you can run Real-Time KQL again, but this time apply a query to simplify the TCP event output you're seeing. If you don't want to go through the prompt questions again, you can instead directly pass in your arguments to the ``run()`` function like so:

```
args = ["etw", "tcp", "--query=SampleQueries\SimplifyEtwTcp.kql", "table"]
run(args)
```

This time, we used the **SampleQueries\SimplifyEtwTcp.kql** file to simplify the TCP output. Here is what the file contains:

```
EtwTcp 
| where EventId in (10, 11)
| extend ProcessName = getprocessname(EventData.PID)
| extend SourceIpAddress = strcat(EventData.saddr, ":", ntohs(EventData.sport))
| extend DestinationIpAddress = strcat(EventData.daddr, ":", ntohs(EventData.dport))
| project SourceIpAddress, DestinationIpAddress, Opcode, ProcessName
```

Real-Time KQL uses the Rx.Kql library to apply real-time processing to event streams. We've added the custom functions ``getprocessname()`` and ``nthos()`` to Rx.Kql to enrich the output that we're seeing.

#### Summarize TCP Event Output:

Finally, to condense the volume of events we're seeing, we can leverage the powerful ``summarize`` feature of the Kusto Query Language to aggregate all our events into 30-second intervals. Run the following in the Python interpreter:

```
args = ["etw", "tcp", "--query=SampleQueries\SummarizeEtwTcp.kql", "table"]
run(args)
```

It is normal to not see any output at first as Rx.Kql is aggregating events into 30-second windows. This is what **SampleQueries\SummarizeEtwTcp.kql** contains:

```
EtwTcp 
| where EventId in (10, 11)
| extend ProcessName = getprocessname(EventData.PID)
| extend SourceIpAddress = strcat(EventData.saddr, ":", ntohs(EventData.sport))
| extend DestinationIpAddress = strcat(EventData.daddr, ":", ntohs(EventData.dport))
| summarize _count = count() by ProcessName, Opcode, SourceIpAddress, DestinationIpAddress, bin(TimeCreated, 30s)
| project _count, SourceIpAddress, DestinationIpAddress, Opcode, ProcessName
```

This query is nearly identical to the one used above, but with an added ``summarize`` line to condense event volume. This feature gives the user the flexibility and power to decide if and how they'd like to reduce event volume before performing any other processing steps.

#### Stop an ETW Trace:

Finally, you can stop the Event Trace Session you started earlier by running the following:

```
logman.exe stop tcp -ets
```



### <a id="Syslog">Local Syslog Event Tracing

This walkthrough demonstrates how to use Real-Time KQL in interactive mode to explore the local syslog authentication log on a Linux machine. (This log is usually stored as **/var/log/auth.log**)

#### Experiment Set-Up:

In one terminal window (Terminal A), navigate to the folder where the Kql Tools are stored. In a second terminal window (Terminal B), prepare to login to your machine via ssh.

#### Look at raw syslog events:

In terminal A, run:

```
tail -f /var/log/auth.log
```

While tail is running in terminal A, use terminal B to try logging into your machine. You can also use terminal B to intentionally fail logging into your machine to see what events are generated on a failed login event.

#### Look at syslog events with Real-Time KQL:

In terminal A,  **start a python shell**, and run the following command to import the Real-Time KQL functionality into your shell:

```
from realtimekql import *
```

Then run:

```
run()
```

The script will then begin to prompt you with questions in order to build your event pipeline. Answer the prompts like so:

```
What input source would you like to use? i.e. 'etw'
syslog

Please specify the argument associated with this input. i.e. the etw session name
/var/log/auth.log

Would you like to apply a query?
no

What type of output would you like to use? i.e. 'table'
json
```

You can terminate the program at any time by pressing **Ctrl + C**.

While RealTimeKql is running in terminal A, use terminal B to try logging in, both successfully and unsuccessfully, to your machine.

#### Simplify local syslog event output:

Continuing within the same python shell, you can run Real-Time KQL again, but this time apply a query to simplify the syslog event output you're seeing. If you don't want to go through the prompt questions again, you can instead directly pass in your arguments to the ``run()`` function like so:

```
args = ["syslog", "/var/log/auth.log", "--query=SyslogLogin.kql", "table"]
run(args)
```

In terminal B, try logging in and out, successfully and unsuccessfully to see some output.

This time, we used the **SyslogLogin.kql** file to simplify the syslog output. Here is what the file contains:

```
Login
| extend Activity = extract("((Failed|Accepted)\\s\\w*)\\sfor\\s(\\w*)", 1, Payload)
| where isnotnull(Activity) and isnotempty(Activity)
| extend User = extract("((Failed|Accepted)\\s\\w*)\\sfor\\s(\\w*)", 3, Payload)
| where isnotnull(Activity) and isnotempty(Activity)
| project User, Activity
```

Real-Time KQL uses the Rx.Kql library to apply real-time processing to event streams.



## <a id="OtherUses">Other Uses of Real-Time KQL for Python

### <a id="CommandLineTool">Regular command line tool:

Real-Time KQL for Python can be run as a regular command line tool and follows the same usage as the main Kql Tools [command line tool](CommandLineTool.md#Usage).

### <a id="DirectComponentUse">Direct component usage:

Real-Time KQL for Python allows the user to bypass the command line interactivity and directly construct the event pipeline of their choosing. The following is a walkthrough example of directly constructing an event pipeline to examine and filter Etw DNS events:

**Start an ETW Trace:**

From within an Administrator command prompt, run the following:

```
logman.exe create trace dns -rt -nb 2 2 -bs 1024 -p {1C95126E-7EEA-49A9-A3FE-A378B03DDB4D} 0xffffffffffffffff -ets
```

**Examine raw DNS events:**

From within the same Administrator command prompt, **start the python shell** and run the following:

```
from realtimekql import *
etw = EtwInterceptor("dns", JsonConsoleOutput())
etw.Start()
```

**Simplify the DNS output:**

*Note: You may have to restart the python shell after examining the raw DNS events. If this happens, make sure to re-import realtimekql before running the following commands:*

```
etw = EtwInterceptor("dns", TableConsoleOutput(), "SampleQueries\EtwDns.kql")
etw.Start()
```

**Stop an ETW Trace:**

```
logman.exe stop dns -ets
```

