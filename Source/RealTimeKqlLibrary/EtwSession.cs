using System;
using System.Security.Principal;

namespace RealTimeKqlLibrary
{
    public class EtwSession: EventComponent
    {
        private readonly string _sessionName;
        public EtwSession(string sessionName, IOutput output, params string[] queries) : base(output, queries)
        {
            _sessionName = sessionName;
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

            // Check if etw session specified exists
            // TODO

            // Getting Etw event stream from Tx.Windows
            var eventStream = Tx.Windows.EtwTdhObservable.FromSession(_sessionName);
            return Start(eventStream, "etw" + _sessionName, true);
        }
    }
}