# Real-Time KQL for Python

To process data with Kusto Query Language (KQL) queries today, users generally have to upload their data to storage first and then query it. The Kql Tools eliminate this need by processing event streams with KQL queries **as events arrive, in real-time.**



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



## <a id="Contributing">Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

