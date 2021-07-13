using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace RealTimeKqlLibrary
{
    public class EvtxFileReader: EventComponent
    {
        private readonly string _fileName;
        public EvtxFileReader(string fileName, IOutput output, params string[] queries) 
            : base(output, queries)
        {
            _fileName = fileName;
        }

        public override bool Start()
        {
            // Check if specified file exists
            if (!File.Exists(_fileName))
            {
                _output.OutputError(new Exception($"ERROR! {_fileName} does not seem to exist."));
                return false;
            }

            var eventStream = Tx.Windows.EvtxObservable.FromFiles(_fileName).Select(x => x.AsDictionary());
            var eventStreamName = _fileName.Split('.');
            return Start(eventStream, eventStreamName[0], false);
        }
    }
}
