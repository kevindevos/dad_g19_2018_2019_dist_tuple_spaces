using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class Client : RemotingEndpoint {
        private int clientRequestSeqNumber;
        public int ClientRequestSeqNumber { get; set; }

        public bool IsBlockedFromSendingRequests { get; private set; }

        private Dictionary<int, Response> receivedResponses;
        public Dictionary<int, Response> ReceivedResponses { get; private set; }

        private Dictionary<int, SemaphoreSlim> _requestSemaphore; 
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0,1);
        
        public Client() : this(defaultClientHost, defaultClientPort) { }

        public Client(string host, int port) : base(host, port, "Client") {
            receivedResponses = new Dictionary<int, Response>();
            _requestSemaphore = new Dictionary<int, SemaphoreSlim>();
            clientRequestSeqNumber = 0;
        }

        public void Write(Tuple tuple) {

            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL, RequestType.WRITE, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;
        }

        public Tuple Read(Tuple tuple) {

            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL,RequestType.READ, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;

            
            WaitForResponse(request.SeqNum);
            return receivedResponses[request.SeqNum].TupleList.First(); //TODO first? what is the order? does it matter?
        }


        public Tuple Take(Tuple tuple) {

            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL, RequestType.TAKE, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;

            WaitForResponse(request.SeqNum);
            return receivedResponses[request.SeqNum].TupleList.First(); //TODO 2-phase locking?
        }

        

        public override void OnReceiveMessage(Message message) {
            if (message.GetType() == typeof(Response)){
                Response response = (Response)message;

                // NOTE: this assumes a server sends back the perfectly correct response that we wanted
                // if the request we sent out has already gotten the respective response ( same seqNumber in request and response of the client ) then ignore
                if (receivedResponses.Keys.Contains(response.Request.SeqNum)) {
                    return;
                }

                // if receives at least one response for a blocking request then unlock ( can only send one and then block, so it will always unblock properly? )
                if (response.Request.RequestType.Equals(RequestType.READ) ||
                    response.Request.RequestType.Equals(RequestType.TAKE))
                {
                    if (_requestSemaphore.TryGetValue(response.Request.SeqNum, out var semaphore))
                    {
                        receivedResponses.Add(response.Request.SeqNum, response);
                        semaphore.Release();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    receivedResponses.Add(response.Request.SeqNum, response);
                }
            }
        }

        public override void OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
        
        // Create semaphore. Wait for response, then disposes and removes.
        private void WaitForResponse(int requestSeqNum)
        {
            _requestSemaphore[requestSeqNum] = new SemaphoreSlim(0,1);
            _requestSemaphore[requestSeqNum].Wait();
            _requestSemaphore[requestSeqNum].Dispose();
            _requestSemaphore.Remove(requestSeqNum);
        }
    }
}
