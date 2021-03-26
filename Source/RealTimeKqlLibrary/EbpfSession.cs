using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RealTimeKqlLibrary
{

    /* TCP Event looks like this:
#pragma pack(push, 1)
	struct tcp_event_t { // event towards the consumer.
	    uint64_t EventTime;
	    uint32_t pid;
	    uint32_t UserId;
	    uint64_t rx_b;
	    uint64_t tx_b;
	    uint32_t tcpi_segs_out;
	    uint32_t tcpi_segs_in;
	    uint16_t family;
	    uint16_t SPT;
	    uint16_t DPT;
	    char task[128];
	    char SADDR[128];
	    char DADDR[128];
	};
#pragma pack(pop)
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct tcpEvent
    {
        public UInt64 EventTime;
        public UInt32 pid;
        public UInt32 UserId;
        public UInt64 rx_b;
        public UInt64 tx_b;
        public UInt32 SentPkts;
        public UInt32 RecvPkts;
        public UInt16 Type;
        public UInt16 SPT;
        public UInt16 DPT;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Command;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string SADDR;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DADDR;
    }

    public class EbpfSession: EventComponent
    {
        private readonly Observable<IDictionary<string, object>> _eventStream;
        private Thread _thread;

        // setupBPF
        [DllImport("event")]
        static extern int setupBPF(char[] ourQueryCharArray);

        // look up symbol: struct tcp_event_t  DequeuePerfEvent()
        [DllImport("event")]
        static extern tcpEvent DequeuePerfEvent();

        private readonly string _source = @"
# include <uapi/linux/ptrace.h>
# include <linux/tcp.h>
# include <net/sock.h> 
# include <bcc/proto.h>
BPF_HASH(birth, struct sock *, u64); 
#pragma pack(push, 1)
struct event_t {
    u64 EventTime;
    u64 ts_us;
    u32 pid;
    u32 uid;
    u64 saddr;
    u64 daddr;
    u64 rx_b;
    u64 tx_b;
    u32 tcpi_segs_out;
    u32 tcpi_segs_in;
    u64 span_us;
    u16 family;
    u16 SPT;
    u16 DPT;
    char task[TASK_COMM_LEN];
};
#pragma pack(pop)
BPF_PERF_OUTPUT(tcpEvents);   
struct id_t {
	u32 pid;
	u32 uid;
	char task[128];
};
BPF_HASH(whoami, struct sock *, struct id_t);  
int kprobe__tcp_set_state(struct pt_regs *ctx, struct sock *sk, int state) {
    u32 pid = bpf_get_current_pid_tgid() >> 32;
    u32 uid =  bpf_get_current_uid_gid() >> 32;
    /*
     * Note to self:
     * As observed,
     * lport is in host byte order, while dport is network byte order.  
     */
    u16 lport = sk->__sk_common.skc_num;
    u16 dport = sk->__sk_common.skc_dport;
    /*
     * This tool includes PID and comm context. It's best effort, and may
     * be wrong in some situations. It currently works like this:
     * - record timestamp on any state < TCP_FIN_WAIT1
     * - cache task context on:
     *       TCP_SYN_SENT: tracing from client
     *       TCP_LAST_ACK: client-closed from server
     * - do output on TCP_CLOSE:
     *       fetch task context if cached, or use current task
     */
    // capture birth time
    if (state < TCP_FIN_WAIT1) {
        /*
         * Matching just ESTABLISHED may be sufficient, provided no code-path
         * sets ESTABLISHED without a tcp_set_state() call. Until we know
         * that for sure, match all early states to increase chances a
         * timestamp is set.
         * Note that this needs to be set before the PID filter later on,
         * since the PID isn't reliable for these early stages, so we must
         * save all timestamps and do the PID filter later when we can.
         */
        u64 ts = bpf_ktime_get_ns();
        birth.update(&sk, &ts);
    }
    // record PID & comm on SYN_SENT
    if (state == TCP_SYN_SENT || state == TCP_LAST_ACK) {
        // now we can PID filter, both here and a little later on for CLOSE
        struct id_t me = {.pid = pid, .uid = uid};
        bpf_get_current_comm(&me.task, sizeof(me.task));
        whoami.update(&sk, &me);
    }
    if (state != TCP_CLOSE)
        return 0;
    // calculate lifespan
    u64 *tsp, delta_us;
    tsp = birth.lookup(&sk);
    if (tsp == 0) {
        whoami.delete(&sk);     // may not exist
        return 0;               // missed create
    }
    delta_us = (bpf_ktime_get_ns() - *tsp) / 1000;
    birth.delete(&sk);
    // fetch possible cached data, and filter
    struct id_t *mep;
    mep = whoami.lookup(&sk);
    if (mep != 0){
        pid = mep->pid;
	uid = mep->uid;
    }
    struct event_t event = { 0 };
    // get throughput stats. see tcp_get_info().
    struct tcp_sock *tp = (struct tcp_sock *)sk;
    event.rx_b = tp->bytes_received;
    event.tx_b = tp->bytes_acked;
    u16 family = sk->__sk_common.skc_family;
    event.tcpi_segs_out = tp->data_segs_out;
    event.tcpi_segs_in = tp->data_segs_in;
	if (family == AF_INET) {
		event.family = AF_INET; 
		event.saddr = sk->__sk_common.skc_rcv_saddr;
		event.daddr = sk->__sk_common.skc_daddr;
	} else if (family == AF_INET6) {
		event.family = AF_INET6;
		bpf_probe_read(&event.saddr, sizeof(event.saddr), sk->__sk_common.skc_v6_rcv_saddr.in6_u.u6_addr32);
		bpf_probe_read(&event.daddr, sizeof(event.daddr), sk->__sk_common.skc_v6_daddr.in6_u.u6_addr32);
	}
    	event.EventTime = bpf_ktime_get_ns();
        event.ts_us = event.EventTime / 1000;
        // a workaround until this compiles with separate lport/dport
        // event.ports = ntohs(dport) + ((0ULL + lport) << 32);
	event.uid =  uid ;
        event.pid = pid;
	event.SPT = lport;
	event.DPT = ntohs(dport);
        if (mep == 0) {
            bpf_get_current_comm(&event.task, sizeof(event.task));
            event.uid =  bpf_get_current_uid_gid() >> 32; /* this is the best we can do here */
        } else {
            bpf_probe_read(&event.task, sizeof(event.task), (void *)mep->task);
        }
	if (event.family){
		tcpEvents.perf_submit(ctx, &event, sizeof(event));
	}
  
    if (mep != 0){
        whoami.delete(&sk);
    }
    return 0;
}
";      
        public EbpfSession(IOutput output, params string[] queries) : base(output, queries)
        {
            _eventStream = new Observable<IDictionary<string, object>>();
        }

        public override bool Start()
        {
            // Setting up pipeline
            if (!Start(_eventStream, "ebpf", true)) return false;

            // Starting reader loop
            _thread = new Thread(RunReaderLoop)
            {
                Priority = ThreadPriority.Highest
            };
            _thread.Start();

            return true;
        }

        private void RunReaderLoop()
        {
            char[] sourceQueryAsCharArray = _source.ToCharArray();
            Thread t = new Thread(() => setupBPF(sourceQueryAsCharArray));
            t.Start();
            Thread.Sleep(3000);

            while (_running)
            {
                var thisEvent = DequeuePerfEvent();
                var txt = JsonConvert.SerializeObject(thisEvent);
                var eventDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(txt);

#if NETCOREAPP3_1
                // converting event time to DateTime object
                if (eventDict.TryGetValue("EventTime", out var nanoSeconds))
                {
                    eventDict["EventTime"] = DateTime.UnixEpoch + new TimeSpan(Convert.ToInt64(nanoSeconds) / 100);
                }
#endif
                _eventStream.Broadcast(eventDict);
            }
        }
    }
}
