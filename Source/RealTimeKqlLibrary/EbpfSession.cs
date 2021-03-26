using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class EbpfSession: EventComponent
    {
        private Observable<IDictionary<string, object>> _eventStream;
        private Thread _thread;
        private bool _running = false;

        public EbpfSession(IOutput output, params string[] queries) : base(output, queries)
        {
            _eventStream = new Observable<IDictionary<string, object>>();
        }

        public override bool Start()
        {
            // Setting up pipeline
            if (!Start(_eventStream, "ebpf", true)) return false;

            // Starting reader loop
            _running = true;
            _thread = new Thread(RunReaderLoop)
            {
                Priority = ThreadPriority.Highest
            };
            _thread.Start();

            return true;
        }

        private void RunReaderLoop()
        {
            while(_running)
            {
                //var thisEvent = DequeuePerfEvent();
                var txt = JsonConvert.SerializeObject(thisEvent);
                var eventDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(txt);

                // converting event time to DateTime object
                if (eventDict.TryGetValue("EventTime", out var nanoSeconds))
                {
                    eventDict["EventTime"] = DateTime.UnixEpoch + new TimeSpan(Convert.ToInt64(nanoSeconds) / 100);
                }

                _eventStream.Broadcast(eventDict);
            }
        }
    }
}
