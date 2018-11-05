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
    class Client : RemotingEndpoint, IRemoting {

        private int clientRequestSeqNumber;
        public int ClientRequestSeqNumber { get; set; }

        private bool isBlockedFromSendingRequests;
        public bool IsBlockedFromSendingRequests { get; private set; }

        private Dictionary<int, Response> receivedResponses;
        public Dictionary<int, Response> ReceivedResponses { get; private set; }
        
        static void Main(string[] args) {
            Client client = new Client();
            client.clientRequestSeqNumber = 0;

        }
        public Client() : this(defaultClientHost, defaultClientPort) { }

        public Client(string host, int port) : base(host, port, "Client") {
            this.receivedResponses = new Dictionary<int, Response>();
        }

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
            Request request = new TakeRequest(clientRequestSeqNumber, endpointURL, tuple);

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

                // NOTE: this assumes a server sends back the perfectly correct response that we wanted
                // if the request we sent out has already gotten the respective response ( same seqNumber in request and response of the client ) then ignore
                if (receivedResponses.Keys.Contains(response.Request.SeqNum)) {
                    return;
                }

                // if receives atleast one response for a blockingrequest then unlock ( can only send one and then block, so it will always unblock properly? )
                if(response.Request.RequestType.Equals(RequestType.READ) || response.Request.RequestType.Equals(RequestType.TAKE)){
                    this.isBlockedFromSendingRequests = false;
                    // TODO parse the response data from the request and do something with it
                }

                receivedResponses.Add(response.Request.SeqNum, response);
            }
        }

        public void OnSendMessage(Message message) {
            // TODO not needed yet
        }
    }
}
