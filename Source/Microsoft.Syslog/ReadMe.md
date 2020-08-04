# Microsoft.Syslog package

The package provides components for implementing [Syslog](https://en.wikipedia.org/wiki/Syslog) facility, both client (producer) and server (listener) parts. The implementation follows the guidelines from the [RFC-5424](https://tools.ietf.org/html/rfc5424) document. 

Major components: 
* **Syslog entry** - a set of classes to represent a syslog payload as a strongly typed object, with nested properties and elements. 
* **Syslog parser and serializer** - facilities to convert *SyslogEntry* object to string payload and vice versa. 
* **Syslog client** - sends syslog entries as UDP packets over the network to the target IP/port. 
* **Syslog listener (server)** - listens to syslog port (514); recieves, reads and parses the syslog messages; broadcasts the syslog entries through IObservable<T> interface 