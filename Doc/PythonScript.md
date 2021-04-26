# Real-Time KQL for Python

## Contents

* [Download & Setup](#Setup)
* [Usage](#Usage)
* [Using Real-Time KQL in Interactive Mode](#InteractiveMode)
  * [Etw TCP Event Tracing](#Etw)
  * [Local Syslog Event Tracing](#Syslog)
* [Other Uses of Real-Time KQL for Python](#OtherUses)
  * [Command line tool](#CommandLineTool)
  * [Direct component use](#DirectComponentUse)



## <a id="Setup">Download & Setup

In an Administrator Command Prompt, Anaconda Prompt, or any elevated terminal window of your choosing, run `pip install realtimekql`. Using a virutal environment of some sort is recommended.

## <a id="Usage">Usage

Real-Time KQL is broken up into three parts: the output, the query, and the input. 

### The Output

Real-Time KQL for Python has a `PythonOutput` class that allows you to customize what happens to events when they are outputted. The simplest usage of the `PythonOutput` class is to instantiate it with no parameters. This will print events to console in JSON format:

```
>>> from realtimekql import *
>>> o = PythonOutput()
```

To customize the output, you can pass in any Python function that takes a dictionary as the only parameter to the `PythonOutput` class. For example, this function stores events in a list to use them later:

```
>>> events = []
>>> def storeEvents(event):
...		events.append(event)
...
>>> from realtimekql import *
>>> o = PythonOutput(storeEvents)
```

The `PythonAdxOutput` class allows you to ingest data to an Azure Data Explorer (Kusto) table through queued ingestion. The class can be instantiated as follows:

```
>>> from realtimekql import *
>>> o = PythonAdxOutput("YourCluster.kusto.windows.net", "YourDatabase", "YourTable", "YourClientId", "YourClientSecret", "YourAuthorityId", resetTable=True)
```



### The Query

You can optionally pass a .kql query into Real-Time KQL to filter, transform, and enrich your events before they even reach the output stage.



### The Input

Real-Time KQL supports various real-time and file input sources. Each input class takes a unique set of arguments, an instance of one of the output classes, as well as an optional path to a query file. This prints real-time Etw TCP events to console in JSON format:

```
>>> from realtimekql import *
>>> o = PythonOutput()
>>> e = EtwSession("tcp", o)
>>> e.Start()
```

Here are all the supported input options and how to use them:

```
EtwSession(sessionName, o, q)
EtlFileReader(filePath, o, q)
WinlogRealTime(logName, o, q)
EvtxFileReader(filePath, o, q)
CsvFileReader(filePath, o, q)
```

The variables `o` and `q` represent the output part and the query part respectively. The query part is optional and can be left out.



