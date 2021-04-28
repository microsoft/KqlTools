# Real-Time KQL Command Line Tool

A command line tool to explore and process real-time streams of events.



## Contents

* [Usage](#Usage)
* [Tracing ETW Tcp Events](#Etw)
* [Tracing Local Syslog Events](#Syslog)



## <a id="Usage">Usage

```
Usage: RealTimeKql <input> [<arg>] [--options] [[<output>] [<args>]] [--query=<path>]
      
input commands
	etw 			<session> 	Listen to real-time ETW session. See Event Trace Sessions in Perfmon
	etl				<file.etl> 	Process the past event in Event Trace File (.etl) recorded via ETW
	winlog 			<logname> 	Listen for new events in a Windows OS log. See Windows Logs in Eventvwr
	evtx    		<file.evtx> 	Process the past events recorded in Windows log file on disk
	csv 			<file.csv> 	Process past events recorded in Comma Separated File
	syslog			<filepath>	Process real-time syslog messages written to local log file
	syslogserver	[options]	Listen to syslog messages on a UDP port

output commands
	json			[file.json]	Optional and default. Events printed to console in JSON format. If filename is specified immediately after, events will be written to the file in JSON format.
	table			Optional, events printed to console in table format
	adx 			<args>		Ingest output to Azure Data Explorer
	blob 			<args>		Ingest output to Azure Blob Storage

query file
	-q|--query 		<file.kql> 	Optional, apply this KQL query to the input stream. If omitted, the stream is propagated without processing to the output
	
Use "RealTimeKql [command] -h|--help" for more information about a command.
```



## <a id="Etw">Tracing ETW TCP Events

This walkthrough demonstrates how to use RealTimeKql to explore TCP events emitted by an Event Tracing for Windows (ETW) provider.

### Start an ETW Trace:

logman is a utility that allows you to start an Event Trace Session for a specific ETW provider or set of providers. In an Administrator command prompt, run this command to start an event trace session for the Etw Tcp provider:

```
logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets
```



### Look at raw TCP data:

From within an Administrator command prompt, navigate to the folder where your RealTimeKql.exe is stored and run the following command:

```
RealTimeKql etw tcp json
```



### Simplify TCP event output:

You can use one of the sample queries provided to simplify the TCP data:

```
RealTimeKql etw tcp --query=SimplifyEtwTcp.kql table
```



### Aggregate TCP events into 30-second intervals:

To reduce the volume of events printing to your screen, you can apply a different query that aggregates events into 30-second windows:

```
RealTimeKql etw tcp --query=SummarizeEtwTcp.kql table
```



### Stop an ETW Trace:

Once you are done exploring events, you can stop the Event Trace Session you started with logman by running the following command:

```
logman.exe stop tcp -ets
```



## <a id="Syslog">Tracing Local Syslog Events

This walkthrough demonstrates how to use RealTimeKql to explore the local syslog authentication log on a Linux machine. (This log is usually stored as **/var/log/auth.log**)

### Experiment Set-Up:

In one terminal window (Terminal A), navigate to the folder where the Kql Tools are stored. In a second terminal window (Terminal B), prepare to login to your machine via ssh.



### Look at raw syslog events:

In terminal A, run:

```
tail -f /var/log/auth.log
```

While tail is running in terminal A, use terminal B to try logging into your machine. You can also use terminal B to intentionally fail logging into your machine to see what events are generated on a failed login event.



### Look at syslog events with Real-Time KQL:

In terminal A, run:

```
sudo ./RealTimeKql syslog /var/log/auth.log json
```

While RealTimeKql is running in terminal A, use terminal B to try logging in, both successfully and unsuccessfully, to your machine.



### Simplify local syslog event output:

You can use one of the sample queries provided to simplify Syslog data. In terminal A, run:

```
sudo ./RealTimeKql syslog /var/log/auth.log --query=SyslogLogin.kql table
```

In terminal B, try logging in and out, successfully and unsuccessfully to see some output.
