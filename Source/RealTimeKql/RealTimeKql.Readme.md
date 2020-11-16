# Real-Time KQL Command Line Tool

Real-Time KQL allows users to examine activity on their machine by directly viewing and querying real-time event streams. Unlike most other tools that offer a similar capability, Real-Time KQL allows a user to begin event processing **as and when events arrive, in real time**.

For instance, suppose a user wanted to see if there was an adversary trying to login into their computer simply by guessing different passwords repeatedly (brute-force). This user could then, for example, use Real-Time KQL to filter through 1000s of events and see only the instances where an adversary has attempted to login into their machine 3 or more times in a 30 second window.

[This query] accomplishes the job mentioned above using syslog events on a Linux machine.

This diagram shows an overview of how Real-Time KQL works:

![StandingQuery.JPG](StandingQuery.JPG)

A user can specify the input and output sources as well as any query files to apply to the given input stream. Real-Time KQL will process the event stream and output the result as another stream to the output source the user had chosen.

[Get started](../../Doc/GettingStarted.md) using Real-Time KQL or see the utility in action:

- [WinLog Demo] *TODO: add link to video demo*
- [Etw Demo] *TODO: add link to video demo*
- [Syslog Demo] *TODO: add link to video demo*