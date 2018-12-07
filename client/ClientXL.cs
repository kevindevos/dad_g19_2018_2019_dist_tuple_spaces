using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

            MulticastMessageWaitAll(View.Nodes, request);
            WaitMessage(request, View.Nodes);
            ClientRequestSeqNumber++;
        }

        public override Tuple Read(Tuple tuple)
        {
            Message resultMessage = null;
            var request = new ReadRequest(ClientRequestSeqNumber, EndpointURL, tuple);

            MulticastMessageWaitAny(View.Nodes, request);

            while (resultMessage == null)
            {
                // wait until it gets a any message 
                WaitMessage(request, View.Nodes);

                lock (ReplyResultQueue)
                {
                    ReplyResultQueue.TryGetValue(request, out var replyResult);
                    resultMessage = replyResult?.GetAnyResult();
                    
                    if (resultMessage == null)
                    {
                        var waitingForReply = replyResult?.GetWaitingForReply();
                        var sendAgain = View.Nodes.Except(waitingForReply);
                        MulticastMessageWaitAny(sendAgain, request);
                    }
                }

            }

            ClientRequestSeqNumber++;
            Response response = (Response) resultMessage;
            return response.Tuples.First();

            // If no response is received after timeout, message is resent 
            /*int timeStep = 200; // ms
            while (true){
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out var responses);
                    if (responses != null && responses.Count >= 1 && responses.First().Tuples.Count > 0){
                        ResponsesReceivedPerRequest.TryRemove(request.SeqNum, out _);
                        return responses.First().Tuples.First();
                    }
                    Thread.Sleep(timeStep);
                }

                // resend same request
                SendMessageToView(request);
            }*/
        }

        public override Tuple Take(Tuple tuple) {
            var takeRequest = new TakeRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            SendMessageToView(takeRequest);
            ClientRequestSeqNumber++;

            int timeStep = 250; // ms
            List<Response> responses = null;
            List<Tuple> intersection = new List<Tuple>();
            Tuple selectedTuple = null;
            
            // PHASE 1
            // Send/Resend the request until all sites have accepted the request and responded with their set of tuples 
            // and the intersection is non null
            do{
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    ResponsesReceivedPerRequest.TryGetValue(takeRequest.SeqNum, out responses);
           
                    // If we got all answers make take progress
                    if (responses != null && responses.Count == View.Size()){
                        ResponsesReceivedPerRequest.TryRemove(takeRequest.SeqNum, out _);
                     
                        // build the intersection 
                        intersection = responses.First().Tuples; 
                        foreach(Response response in responses){
                            IEnumerable<Tuple> enumerableIntersection = intersection.Intersect(response.Tuples).ToList();
                        }
                        
                        
                        // Choose random tuple from intersection
                        selectedTuple = intersection.ElementAt((new Random()).Next(0, intersection.Count));
            
                        // PHASE 2
                        // Issue a multicast remove for the selectedTuple
                        Message takeRemove = new TakeRemove(EndpointURL, selectedTuple, takeRequest.SeqNum);
                        SendMessageToView(takeRemove);
                        
                        // wait for all acks
                        int ackCount = 0;
                        do {
                            for (int j = 0; j < DefaultTimeoutDuration; j += timeStep){
                                AcksReceivedPerRequest.TryGetValue(takeRequest.SeqNum, out ackCount);
                                if (ackCount == View.Size()){
                                    AcksReceivedPerRequest.TryRemove(takeRequest.SeqNum, out _);
                                    return selectedTuple;
                                }
                                Thread.Sleep(timeStep);
                            }
                            // Resend remove
                            SendMessageToView(takeRemove);
                        } while (ackCount < View.Size());

                        return null;
                    }
                    
                    Thread.Sleep(timeStep);
                }
                
                // ask servers to release their locks since at this point the take request has been rejected 
                // because we didn't get all the responses within timeout period
                Message lockRelease = new LockRelease(EndpointURL, takeRequest.SeqNum);
                SendMessageToView(lockRelease);

                // resend the same request
                SendMessageToView(takeRequest);
            } while (responses == null || (responses.Count < View.Size() && intersection.Count == 0));

            return null;

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
            
            if(ack.Message.GetType() == typeof(TakeRemove)) {
                TakeRemove takeRemove = (TakeRemove)ack.Message;
                AcksReceivedPerRequest.TryRemove(takeRemove.TakeRequestSeqNumber, out var oldCounter);
                AcksReceivedPerRequest.TryAdd(takeRemove.TakeRequestSeqNumber, ++oldCounter);
            }
        }

        private void SendMessageToView(Message message) {
            SendMessageToView(View.Nodes , message);
        }

    }
}
