using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.Behaviour;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace {
    public abstract class Server : RemotingEndpoint, ITupleOperations {
        public delegate Message SendMessageDelegate(Message message);

        // TODO move variables and logic from Server to ServerSMR that doesn't belong here

        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        public ServerBehaviour Behaviour { get; private set; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public Dictionary<string, int> MostRecentClientRequestSeqNumbers;

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> RequestList { get; }

        public int LastOrderSequenceNumber { get; set; }

        public List<RemotingEndpoint> OtherServers { get; private set; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Request> LastExecutedRequests { get; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Order> LastExecutedOrders { get; private set; }

        public List<Order> SavedOrders { get; }

        protected List<Ack> ReceivedAcks { get; private set; }

        public string MasterEndpointURL { get; set; }

        // Tuple space
        private TupleSpace TupleSpace { get; }

        protected Server(string host, int port) : base(host, port, "Server") {
            Port = port;
            TupleSpace = new TupleSpace();
            RequestList = new List<Request>();
            LastExecutedRequests = new ConcurrentDictionary<string, Request>();
            LastExecutedOrders = new ConcurrentDictionary<string, Order>();
            SavedOrders = new List<Order>();
            LastOrderSequenceNumber = 0; 
        }

        protected Server() : this(DefaultServerHost, DefaultServerPort) { }

        private Server(int serverPort) : this(DefaultServerHost, serverPort) { }

        // ITupleOperations Methods
        public void Write(Tuple tuple)
        {
            TupleSpace.Write(tuple);
            Log("Wrote : " + tuple);
        }

        public List<Tuple> Read(TupleSchema tupleSchema)
        {
            var listTuple = TupleSpace.Read(tupleSchema);
            Log("Read (first tuple): " + listTuple.First());
            return listTuple;
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            // TODO 2 phase lock - first list what there is and lock those, only then on confirm the client takes them?
            var listTuple = TupleSpace.Take(tupleSchema);;
            Log("Took (first tuple): " + listTuple.First());
            return listTuple;
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

        public virtual void Log(string text) {
            Console.WriteLine("[SERVER:"+EndpointURL +"] : " + text);
        }
      
    }

}