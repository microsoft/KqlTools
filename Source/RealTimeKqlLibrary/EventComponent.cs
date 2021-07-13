using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace RealTimeKqlLibrary
{
    // Class to help set up event pipeline
    public abstract class EventComponent
    {
        // Input
        private IObservable<IDictionary<string, object>> _eventStream;

        // Event processing
        private EventProcessor _eventProcessor;
        private readonly string[] _queries;

        // Output
        protected IOutput _output;
        private IDisposable _outputSubscription;

        // Special cases
        private IDisposable _intermediateSubscription;

        // Clean up
        private bool _running;
        private readonly ConsoleCancelEventHandler _cancelEventHandler;

        protected EventComponent(IOutput output, params string[] queries)
        {
            _running = true;
            _output = output;
            _queries = queries;

            _eventStream = null;
            _eventProcessor = null;
            _outputSubscription = null;
            _intermediateSubscription = null;

            // Setting up clean-up code for exit
            _cancelEventHandler = delegate (object sender, ConsoleCancelEventArgs eventArgs) {
                if (_running)
                {
                    Stop();
                }
                else
                {
                    Console.CancelKeyPress -= _cancelEventHandler;
                }
            };
            Console.CancelKeyPress += _cancelEventHandler;
        }

        // Used to start the event pipeline and can be called directly by the user
        public abstract bool Start();

        // Called by derived classes to set up event pipeline subscriptions
        protected bool Start(
            IObservable<IDictionary<string, object>> eventStream,
            string eventStreamName,
            bool realTimeMode,
            ISubject<IDictionary<string, object>> subject=null)
        {
            _eventStream = eventStream;

            // Sending event stream to next stage in event pipeline
            if(subject != null)
            {
                // Input stream goes to custom actions
                _intermediateSubscription = _eventStream.Subscribe(subject.OnNext, subject.OnError, subject.OnCompleted);

                // Custom actions go to output
                _outputSubscription = subject.Subscribe(_output.OutputAction, _output.OutputError, Stop);
            }
            else if (_queries == null || _queries.Length == 0 || string.IsNullOrEmpty(_queries[0]))
            {
                // Input stream goes straight to output
                _outputSubscription = _eventStream.Subscribe(_output.OutputAction, _output.OutputError, Stop);
            }
            else
            {
                // Input stream goes through Rx.Kql before hitting output
                _eventProcessor = new EventProcessor(
                    _eventStream, eventStreamName, 
                    _output, 
                    realTimeMode, 
                    _queries);
                return _eventProcessor.ApplyRxKql();
            }

            return true;
        }

        // Called when user terminates program with Ctrl + C or event stream completes
        public void Stop()
        {
            _running = false;
            _output.OutputCompleted();

            // Disposing subscriptions
            if(_intermediateSubscription != null)
            {
                _intermediateSubscription.Dispose();
            }
            if (_outputSubscription != null)
            {
                _outputSubscription.Dispose();
            }
            if (_eventProcessor != null)
            {
                _eventProcessor.Stop();
            }

            // Stopping output
            _output.Stop();
        }
    }
}