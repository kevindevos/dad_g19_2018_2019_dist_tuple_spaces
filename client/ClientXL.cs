using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientXL : Client {
        // request seq number, counter
        private ConcurrentDictionary<int, int> AcksReceivedPerRequest;

        // request seq number, responses
        private ConcurrentDictionary<int, List<Response>> ResponsesReceivedPerRequest;

        private const int DefaultTimeoutDuration = 5000; // ms

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
        }

        public ClientXL(string host, int port) : this(BuildRemoteUrl(host, port, ClientObjName)) {
        }

        public ClientXL(string remoteUrl) : base(remoteUrl) {
            AcksReceivedPerRequest = new ConcurrentDictionary<int, int>();
            ResponsesReceivedPerRequest = new ConcurrentDictionary<int, List<Response>>();
        }

        public override void Write(Tuple tuple) {
            var request = new WriteRequest(ClientRequestSeqNumber, EndpointURL, tuple);

            SendMessageToView(request);
            ClientRequestSeqNumber++;

            // Count Acks received from servers, if a timeout is reached, the message is resent
            int acksReceived = 0;
            int timeStep = 200; // ms
            do{
                // did we get all acks?
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    AcksReceivedPerRequest.TryGetValue(request.SeqNum, out acksReceived);
                    if (acksReceived == View.Count){
                        return;
                    }
                    Thread.Sleep(timeStep);
                }
                // timeout reached resend the same request
                SendMessageToView(request);
            } while (acksReceived < View.Count);
            
        }

        public override Tuple Read(Tuple tuple) {
            var request = new ReadRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            SendMessageToView(request);
            ClientRequestSeqNumber++;

            // If no response is received after timeout, message is resent 
            int timeStep = 200; // ms
            while (true){
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out var responses);
                    if (responses != null && responses.Count >= 1 && responses.First().Tuples.Count > 0){
                        return responses.First().Tuples.First();
                    }
                    Thread.Sleep(timeStep);
                }

                // resend same request
                SendMessageToView(request);
            }
        }

        public override Tuple Take(Tuple tuple) {
            var takeRequest = new TakeRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            SendMessageToView(takeRequest);
            ClientRequestSeqNumber++;

            int timeStep = 250; // ms
            List<Response> responses = null;
            List<Tuple> intersection = new List<Tuple>();
            
            // PHASE 1
            // Send/Resend the request until all sites have accepted the request and responded with their set of tuples 
            // and the intersection is non null
            do{
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    ResponsesReceivedPerRequest.TryGetValue(takeRequest.SeqNum, out responses);
                    if (responses.Count == View.Count){
                        intersection = responses.First().Tuples; // start point for intersection
                        foreach(Response response in responses) {
                            intersection = intersection.Intersect(response.Tuples).ToList(); // get common tuples in all response lists
                        }
                    }
                    else{
                        Thread.Sleep(timeStep);
                    }
                }
                
                // ask servers to release their locks since at this point the take request has been rejected 
                // because we didn't get all the responses within timeout period
                Message lockRelease = new LockRelease(EndpointURL, takeRequest.SeqNum);
                SendMessageToView(lockRelease);

                // resend the same request
                SendMessageToView(takeRequest);
            } while (responses.Count < View.Count && intersection.Count == 0);

            // Choose random tuple from intersection
            Tuple selectedTuple = intersection.ElementAt((new Random()).Next(0, intersection.Count));
            
            // PHASE 2
            // Issue a multicast remove for the selectedTuple
            Message takeRemove = new TakeRemove(EndpointURL, selectedTuple, takeRequest.SeqNum);
            SendMessageToView(takeRemove);
            ClientRequestSeqNumber++; 

            int ackCount = 0;
            do {
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    AcksReceivedPerRequest.TryGetValue(takeRequest.SeqNum, out ackCount);
                    if (ackCount == View.Count){
                        return selectedTuple;
                    }
                    Thread.Sleep(timeStep);
                }
                // Resend remove
                SendMessageToView(takeRemove);
            } while (ackCount < View.Count);

            return selectedTuple;
        }

        
        
        public override Message OnReceiveMessage(Message message) {
            if(message.GetType() == typeof(Ack)  ) {
                UpdateAckCounter((Ack) message);
                
            }
            if(message.GetType() == typeof(Response)) {
                Response response = (Response) message;
                ResponsesReceivedPerRequest.TryRemove(response.Request.SeqNum, out var storedResponses);
                
                if (storedResponses == null){
                    storedResponses = new List<Response>();
                }
                
                storedResponses.Add(response);
                ResponsesReceivedPerRequest.TryAdd(response.Request.SeqNum, storedResponses);
            }

            return null;
        }

        private void UpdateAckCounter(Ack ack){
            if(ack.Message.GetType().IsSubclassOf(typeof(Request))) {
                Request request = (Request)ack.Message;
                AcksReceivedPerRequest.TryRemove(request.SeqNum, out var oldCounter);
                AcksReceivedPerRequest.TryAdd(request.SeqNum, ++oldCounter);
            }
        }

        private void SendMessageToView(Message message) {
            SendMessageToView(View, message);
        }

        

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
