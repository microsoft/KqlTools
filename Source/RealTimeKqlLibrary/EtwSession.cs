using Kusto.Cloud.Platform.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace RealTimeKqlLibrary
{
    public class EtwSession: EventComponent
    {
        private readonly string _sessionName;
        private readonly bool _kqlProcessing;
        public EtwSession(string sessionName, IOutput output, params string[] queries) : base(output, queries)
        {
            _sessionName = sessionName;

            if(queries == null || queries.Length == 0 || string.IsNullOrEmpty(queries[0]))
            {
                _kqlProcessing = false;
            }
            else
            {
                _kqlProcessing = true;
            }
        }

        override public bool Start()
        {
            // Check if user is administrator
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("ERROR! To attach to a real-time ETW session, you must be Administrator.");
                return false;
            }

            // Getting Etw event stream from Tx.Windows
            var eventStream = Tx.Windows.EtwTdhObservable.FromSession(_sessionName);

            ModifierSubject<IDictionary<string, object>> modifier = null;
            if(!_kqlProcessing)
            {
                // Events go to output as Tx.Windows.EtwTdhEvent, 
                // which implements lazy deserialization
                // Need to force total deserialization before events hit output
                // to avoid termination errors about accessing protected memory

                modifier = new ModifierSubject<IDictionary<string, object>>(OutputAction);
            }

            return Start(eventStream, "etw" + _sessionName, true, modifier);
        }

        private IDictionary<string, object> OutputAction(IDictionary<string, object> obj)
        {
            obj.GetEnumerator();
            return obj;
        }
    }
}
