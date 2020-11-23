# Real-Time KQL
![.NET Core Desktop](https://github.com/microsoft/KqlTools/workflows/.NET%20Core%20Desktop/badge.svg?branch=master&event=push)

In order to view event logs today, users generally have to rely on tools that will first upload their data to storage **and then** query it. With Real-Time KQL, this is no longer necessary. Event processing happens **as events arrive, in real-time**.



![Doc/StandingQuery.jpg](Doc/StandingQuery.jpg)



[**Get started**](Doc/GettingStarted.md) right away with using Real-Time KQL or learn [**how it works**](Doc/Readme.md).



<div align="center">
    <a href="https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-winx64-TestRelease.zip"><img src="DownloadWindowsButton.png" width="40%"/></a>&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-linux-TestRelease.zip"><img src="DownloadLinuxButton.png" width="40%"/></a>
</div>



### Demos and Documentation

|           WinLog (Windows)           | Etw (Windows) |            Syslog (Linux)            |
| :----------------------------------: | :-----------: | :----------------------------------: |
| [Demo](https://youtu.be/GoTSuWPrkig) |     Demo      | [Demo](https://youtu.be/kw6bSGolnpU) |
|         [Doc](Doc/Winlog.md)         | [Doc](Etw.md) |         [Doc](Doc/Syslog.md)         |



### Query Files

Check out the [query writing guide](Doc/QueryGuide.md) for some best practices on coming up with queries for Real-Time KQL.



### Output Options

|                       Real-Time Output                       |                         File Output                          |                        Upload Output                         |
| :----------------------------------------------------------: | :----------------------------------------------------------: | :----------------------------------------------------------: |
| [consoleOutput](Doc/RealTimeOutput.md#ConsoleOutput) - Results printed to standard output | [jsonOutput](Doc/FileOutput.md#JSONOutput) - Each event is a JSON dictionary | [adxOutput](Doc/UploadOutput.md#ADXOutput) - Upload to Kusto (Azure Data Explorer) |
| **webEvents** - Real-Time KQL acts as real-time server for events. | **csvOutput** - Each event is a row in Comma Separated Value table | [blobStorage](Doc/UploadOutput.md#BlobStorage) - Upload as JSON objects to BlobStorage |
|                                                              | **htmlOutput** - Each event formatted as human-readable DIV element |                                                              |



## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.