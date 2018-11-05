using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.Behaviour;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace.Server{
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

        private List<IRemoting> otherServers;
        public List<IRemoting> OtherServers { get; private set; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Request> LastExecutedClientRequests;

        // Tuple space
        private readonly TupleSpace _tupleSpace;
        private readonly int port;

        private TcpChannel _tcpChannel;

        public Server(string host, int port) : base(host, port, "Server") {
            this.port = port;
            _tupleSpace = new TupleSpace();
            RequestList = new List<Request>();
            LastExecutedClientRequests = new ConcurrentDictionary<string, Request>();
        }
        public Server() : this(defaultServerHost, 8086) { }

        private Server(int serverPort) : this(defaultServerHost, serverPort) { }

       
        // IRemoting Methods 
        public void OnReceiveMessage(Message message) {
            Behaviour.ProcessMessage(message);
        }
        
        public void OnSendMessage(Message message)
        {
            // TODO not needed yet
        }

            
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
      
    }

}