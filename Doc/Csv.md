# CSV

### Parsing a CSV File

The greatest advantage to using Real-Time KQL for parsing a CSV file is the ability to use KQL queries to prepossess your data before parceling it off to your desired destination. Below is a basic example of using the csv file input mode. To add a query to this command, you can simply specify the `--query` option and provide the name of the file containing your query. For more details on creating queries, see the [Query Guide](QueryGuide.md).

**Example usage**:

` RealTimeKql csv --file=Sample.csv --outputconsole `

**Example breakdown**:

* `--file=Sample.csv` : the CSV file you want Real-Time KQL to parse
* `--outputconsole`: output the results to console