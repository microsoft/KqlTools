using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public interface IOutput
    {
        // output action for KqlNodeHub
        void KqlOutputAction(KqlOutput obj);

        // output action for directly handling events (when Rx.Kql is not used)
        void OutputAction(IDictionary<string, object> obj);

        // output action for directly handling errors  (when Rx.Kql is not used)
        void OutputError(Exception ex);

        // called when user terminates program 
        void Stop();
    }
}
