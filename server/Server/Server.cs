using System;
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
    public abstract class Server : MarshalByRefObject, IRemoting, ITupleOperations {
        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        protected IServerBehaviour Behaviour;

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public readonly Dictionary<string, int> MostRecentClientRequestSeqNumbers;

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        protected internal readonly List<Request> RequestList;

        // Tuple space
        private readonly TupleSpace _tupleSpace;
        private readonly int _serverPort;

        private TcpChannel _tcpChannel;
        
        protected Server() : this(8086){
        }

        private Server(int serverPort) {
            _serverPort = serverPort;
            _tupleSpace = new TupleSpace();
            RequestList = new List<Request>();
            MostRecentClientRequestSeqNumbers = new Dictionary<string, int>();

            RegisterTcpChannel();
            RegisterService();
        }

       
        // IRemoting Methods 
        public void OnReceiveMessage(Message message) {
            Behaviour.OnReceiveMessage(message);
        }
        
        public void OnSendMessage(Message message)
        {
            Behaviour.OnSendMessage(message);
        }

        public void RegisterTcpChannel() {
            _tcpChannel = new TcpChannel(_serverPort);
            ChannelServices.RegisterChannel(_tcpChannel, false);
        }

        public void RegisterService() {
            RemotingServices.Marshal(this, "Server", typeof(Server));
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
            return _tupleSpace.Take(tupleSchema);
        }
        
                
        // Request Management Methods
        public void RemoveRequestFromList(Request request) {
            RequestList.Remove(request);
        }
           
        public Request GetRequestBySeqNumberAndClientUrl(int seq, string clientUrl) {
            for(var i = 0; i < RequestList.Capacity; i++) {
                var temp = RequestList[i];
                if(temp.SeqNum == seq && temp.ClientRemoteUrl.Equals(clientUrl)){
                    return temp;
                }
            }
            return null;
        }
        
        public void Decide()
        {
            lock (RequestList) {
                // TODO
                // decide from the list of requests if server can do something or not
            }        
        }
        
        // Auxiliary Methods        
        public string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }



    }

}