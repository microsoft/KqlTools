using System.Linq;
using System.Reactive.Linq;

namespace RealTimeKqlLibrary
{
    public class WinlogRealTime : EventComponent
    {
        private readonly string _logName;
        public WinlogRealTime(string logName, IOutput output, params string[] queries) : base(output, queries)
        {
            _logName = logName;
        }

        public override bool Start()
        {
            var eventStream = Tx.Windows.EvtxObservable.FromLog(_logName, null, false)
                .Select(x => x.AsDictionary());
            return Start(eventStream, "log" + _logName, true);
        }
    }
}
