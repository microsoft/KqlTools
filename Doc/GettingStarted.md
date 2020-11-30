# Getting Started with Real-Time KQL

This guide will walk you through the process of downloading and setting up the KqlTools suite for use on either a Windows or Linux machine. Start by downloading the appropriate files for your machine:

<div align="center">
    <a href="https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-winx64-TestRelease.zip"><img src="../DownloadWindowsButton.png" width="40%"/></a>&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-linux-TestRelease.zip"><img src="../DownloadLinuxButton.png" width="40%"/></a>
</div>

**Jump To:**

* [Windows](#Windows)
  * [Prerequisites](#WinPreReq)
  * [Download and Setup](#WinDownSet)
  * [Running Real-Time KQL](#WinRun)
* [Linux](#Linux)
  * [Prerequisites](#LinuxPreReq)
  * [Download and Setup](#LinuxDownSet)
  * [Running Real-Time KQL](#LinuxRun)



## <a id="Windows"></a>Windows

### <a id="WinPreReq"></a>Prerequisites

1. Install [.NET Core SDK 3.1.200](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.200-windows-x64-installer)

### <a id="WinDownSet"></a>Download and Setup

1. [Download](https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-winx64-TestRelease.zip) and extract the program files for Windows. (You can also download the files using the link at the top of the page.)
2. Open a Command Prompt as Administrator and navigate to the folder where you've extracted the files.
3. Navigate into the `win-x64` folder. This is the folder from which you will run Real-Time KQL.

### <a id="WinRun"></a>Running Real-Time KQL

1. From within the `win-x64` folder, run the following command to get an overview of your options:
```bash
RealTimeKql --help
```
2. For more information and examples on using Real-Time KQL for Windows:
   - [winlog](Winlog.md): OS or application logs you see in EventVwr or log file(s) on disk
   - [etw](Etw.md): real-time session in Event Tracing for Windows (ETW) or previously recorded "Event Trace Log"



## <a id="Linux"></a>Linux

### <a id="LinuxPreReq"></a>Prerequisites

#### Install .NET Core 3.1

1. Add the Microsoft package signing key to your list of trusted keys and add the package repository. Open a terminal and run the following commands:

```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```

2. Install the .NET SDK:

```bash
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
```



### <a id="LinuxDownSet"></a>Download and Setup

1. [Download](https://github.com/microsoft/KqlTools/releases/download/v1.0.0/RealTimeKql-linux-TestRelease.zip) and extract the program files for Linux. (You can also download the files using the link at the top of the page.)
2. Open a terminal window and navigate to the folder where you've extracted the files.
3. Navigate into the `RealTimeKql-linux-TestRelease` folder. This is the folder from which you will run Real-Time KQL.

### <a id="LinuxRun"></a>Running Real-Time KQL

1. From within the `RealTimeKql-linux-TestRelease` folder, run the following command to get an overview of your options:

```bash
sudo ./RealTimeKql syslog --help
```

2. For more information and examples on using Real-Time KQL for Linux, see the [syslog guide](Syslog.md).