using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Tuple = CommonTypes.Tuple;

namespace ClientNamespace {
    public class Client : RemotingEndpoint, IRemoting {

        private int clientRequestSeqNumber;
        public int ClientRequestSeqNumber { get; set; }

        private bool isBlockedFromSendingRequests;
        public bool IsBlockedFromSendingRequests { get; private set; }
        
        static void Main(string[] args) {
            Client client = new Client();
            client.clientRequestSeqNumber = 0;

        }
        public Client() : base("Client") { }

        public Client(string host, int port) : base(host, port, "Client") { }

        public void Write(Tuple tuple) {
            // remote exceptions?
            Request request = new WriteRequest(clientRequestSeqNumber, endpointURL, tuple);

            SendRequestToKnownServers(request);
        }

        public void Read(Tuple tuple) {
            // remote exceptions?
            Request request = new ReadRequest(clientRequestSeqNumber, endpointURL, tuple);

            SendRequestToKnownServers(request);

            this.isBlockedFromSendingRequests = true;
        }


        public void Take(Tuple tuple) {
            // remote exceptions?
            Request request = new ReadRequest(clientRequestSeqNumber, endpointURL, tuple);

            SendRequestToKnownServers(request);

            this.isBlockedFromSendingRequests = true;
        }

        public void SendRequestToKnownServers(Request request) {
            RemoteAsyncDelegate remoteDel;

            for (int i = 0; i < knownServerRemotes.Capacity; i++) {
                remoteDel = new RemoteAsyncDelegate(knownServerRemotes.ElementAt(i).OnReceiveMessage);
                remoteDel.BeginInvoke(request, null, null);
            }
            clientRequestSeqNumber++;
        }

        public void OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Response))){
                Response response = (Response)message;

                // if receives atleast one response for a blockingrequest? then unlock
                if(response.Request.RequestType.Equals(RequestType.READ) || response.Request.RequestType.Equals(RequestType.TAKE)){
                    this.isBlockedFromSendingRequests = false;
                    // TODO parse the response data from the request and do something with it
                }
            }
        }

        public void OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
