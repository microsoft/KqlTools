using System;
using System.IO;
using System.Reactive.Linq;

namespace RealTimeKqlLibrary
{
    public class EtlFileReader : EventComponent
    {
        private readonly string _fileName;

        public EtlFileReader(string fileName, IOutput output, params string[] queries) : base(output, queries)
        {
            _fileName = fileName;
        }
        override public bool Start()
        {
            // Check if specified file exists
            if (!File.Exists(_fileName))
            {
                Console.WriteLine($"ERROR! {_fileName} does not seem to exist.");
                return false;
            }

            var eventStream = Tx.Windows.EvtxObservable.FromFiles(_fileName)
                .Select(x => x.AsDictionary());
            var eventStreamName = _fileName.Split('.');
            return Start(eventStream, eventStreamName[0], false);
        }
    }
}
