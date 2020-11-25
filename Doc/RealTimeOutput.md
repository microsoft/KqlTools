# Real-Time Output

Real-Time KQL supports real-time outputs. The output is treated as a stream and can be infinite.

**Jump To:**

* [Console Output](#ConsoleOutput)
* [Web Events](#WebEvents)

## <a id="ConsoleOutput"></a>Console Output

With the console output option, the results are printed on screen (standard output). The results will roll-off depending on how you've set up the console window buffer.

**Example Usage**

`sudo ./RealTimeKql syslog --logfile=/var/log/auth.log --outputconsole `

**Example Usage Breakdown**

* `--logfile=/var/log/auth.log` : attach Real-Time KQL to the `/var/log/auth.log` file
* `--outputconsole` : print the results to console



## <a id="WebEvents"></a>Web Events

*Coming soon*