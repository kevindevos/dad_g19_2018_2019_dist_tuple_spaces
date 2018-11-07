using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.Behaviour;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace {
    public abstract class Server : RemotingEndpoint, ITupleOperations {
        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        public ServerBehaviour Behaviour;

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public readonly Dictionary<string, int> MostRecentClientRequestSeqNumbers;

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> RequestList;

        private int lastOrderSequenceNumber;
        public int LastOrderSequenceNumber { get; set; }

        private List<RemotingEndpoint> otherServers;
        public List<RemotingEndpoint> OtherServers { get; private set; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Request> LastExecutedRequests;

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Order> LastExecutedOrders;

        public List<Order> SavedOrders;

        // Tuple space
        private readonly TupleSpace _tupleSpace;

        public Server(string host, int port) : base(host, port, "Server") {
            this.port = port;
            _tupleSpace = new TupleSpace();
            RequestList = new List<Request>();
            LastExecutedRequests = new ConcurrentDictionary<string, Request>();
            LastExecutedOrders = new ConcurrentDictionary<string, Order>();
            SavedOrders = new List<Order>();
            LastOrderSequenceNumber = 0; // resets the counter if the server went down and had to be recreated!, we'd need to know which orders were executed here before if there's persistency,
            //or we need to ask for ALL previous orders..
        }

        public Server() : this(defaultServerHost, defaultServerPort) { }

        private Server(int serverPort) : this(defaultServerHost, serverPort) { }

        // ITupleOperations Methods
        public void Write(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
        }

        public List<Tuple> Read(TupleSchema tupleSchema)
        {
            return _tupleSpace.Read(tupleSchema);
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            // TODO 2 phase lock - first list what there is and lock those, only then on comfirm the client takes them?
            return _tupleSpace.Take(tupleSchema);
        }

        public void SaveRequest(Request request) {
            lock (RequestList) {
                RequestList.Add(request);
            }
        }

        public void DeleteRequest(Request request) {
            lock (RequestList) {
                RequestList.Remove(request);
            }
        }

        public void Log(string text) {
            Console.WriteLine("[SERVER:"+endpointURL +"] : " + text);
        }
      
    }

}