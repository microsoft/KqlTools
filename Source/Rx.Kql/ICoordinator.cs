// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;

    public interface ICoordinator
    {
        // Methods that are called by management applications
        NodeTypeStatistics[] GetNodeTypes();

        NodeStatus[] GetNodeStatus(string nodeSubsetKql);

        void SendCommand(NodeCommand command);

        // Methods that are called by processing nodes
        void NodeStatus(NodeStatus status);

        NodeCommand[] GetCommands(string nodeType, IDictionary<string, object> nodeInfo, DateTime lastCommandUtc);
    }

    public class NodeStatus
    {
        public DateTime NodeTimeUtc { get; set; } // Node local clock timestamp in UTC

        public string NodeType { get; set; } // Node type such as WecCollector or Source

        public string NodeId { get; set; } // Stable identifier such as the first netwok card MAC address or Azure VM-Id

        public DateTime LastCommandUtc { get; set; } // Timestamp of the last command known to the node

        public dynamic NodeInfo { get; set; } // Information about the node as JSON
    }

    public class NodeTypeStatistics
    {
        public string NodeType { get; set; }

        public long Count { get; set; }
    }

    public class NodeCommand
    {
        public DateTime TimestampUtc { get; set; } // The Timestamp of the command

        public string NodeType { get; set; } // Node type such as WecCollector or Source

        public string NodeSubsetKql { get; set; } // KQL expression what nodes this command applies to. E.g. Region == "EU"

        public string Command { get; set; } // General command category, such as Rx.KQL or Upload

        public dynamic Arguments { get; set; } // Arguments such as the csl query or target cluster/databse to upload
    }
}