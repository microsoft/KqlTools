# File Output

Real-Time KQL supports writing output to files. The output is treated as a stream and can be infinite.

**Jump To:**

* [JSON Output](#JSONOutput)
* [CSV Output](CSVOutput)
* [HTML Output](HTML Output)

## <a id="JSONOutput"></a>JSON Output

With a JSON output, each event is converted into a JSON dictionary.

**Example usage - Monitoring the Security Windows log:**

`RealTimeKql winlog --log="Security" --outputjson="Security.json"`

**Example breakdown:**

* `--log="Security"` : monitor the Security log
* `--outputjson="Security.json"` : output results to `Security.json`



## <a id="CSVOutput"></a>CSV Output

*Coming soon*



## <a id="HTMLOutput"></a>HTML Output

*Coming soon*