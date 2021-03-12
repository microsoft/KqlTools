# Real-Time KQL

To process data with Kusto Query Language (KQL) queries today, users generally have to upload their data to storage first and then query it. The Kql Tools eliminate this need by processing event streams with KQL queries **as events arrive, in real-time.**



## Contents

* [List of tools](#Tools)
* [Supported event sources](#Inputs)
* [Supported event destinations](#Outputs)
* [About](#About)
* [Contributing](#Contributing)



## <a id="Tools">List of Tools

|             **Command Line Tool**             |             **Python Script**              |
| :-------------------------------------------: | :----------------------------------------: |
|    [Documentation](Doc/CommandLineTool.md)     |    [Documentation](Doc/PythonScript.md)    |
| [Downloads](Doc/CommandLineTool.md#Downloads) | [Downloads](Doc/PythonScript.md#Downloads) |
|             Demo (*coming soon*)              |            Demo (*coming soon*)            |



## <a id="Inputs">Supported Event Sources

In addition to processing **CSV files**, the KQL tools support the following input sources:

|                         |                          Windows                          |                            Linux                             |
| :---------------------: | :-------------------------------------------------------: | :----------------------------------------------------------: |
|       **OS Logs**       | **WinLog** - logs seen in EventVwr or log file(s) on disk |                   **Syslog** - the OS log                    |
| **High-Volume Tracing** |            **Etw** - Event Tracing for Windows            | **EBPF** - dynamic interception of kernel and user mode functions (*Coming soon*) |



## <a id="Outputs">Supported Event Destinations

|                       Real-Time Output                       |                       File Output                       |                  Upload Output                   |
| :----------------------------------------------------------: | :-----------------------------------------------------: | :----------------------------------------------: |
| **json**- Results printed to standard output in JSON format  |  **jsonfile** - Results written to file in JSON format  | **adx** - Upload to Kusto (Azure Data Explorer)  |
| **table** - Results printed to standard output in table format | **tablefile** - Results written to file in table format | **blob** - Upload as JSON objects to BlobStorage |



## <a id="About">About

The Kql Tools allow users to examine activity on their machine by directly viewing and querying real-time event streams.

![StandingQuery.jpg](StandingQuery.jpg)



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

