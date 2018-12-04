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
        private ConcurrentDictionary<int, int> AckReceivedPerRequest;

        // request seq number, responses
        private ConcurrentDictionary<int, List<Response>> ResponsesReceivedPerRequest;

        private const int DefaultTimeoutDuration = 5;

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
        }

        public ClientXL(string host, int port) : this(BuildRemoteUrl(host, port, ClientObjName)) {
        }

        public ClientXL(string remoteUrl) : base(remoteUrl) {
            AckReceivedPerRequest = new ConcurrentDictionary<int, int>();
        }

        public override void Write(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            SendMessageToView(request);
            ClientRequestSeqNumber++;

            // Count Acks received from servers, if a timeout is reached, the message is resent
            int acksReceived = 0;
            int timeStep = 200; // ms
            do{
                // did we get all acks?
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    if (acksReceived < View.Count){
                        Thread.Sleep(timeStep);
                    }
                }
                // timeout reached resend the same request
                SendMessageToView(request);
            } while (acksReceived < View.Count);
        }

        public override Tuple Read(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);
            SendMessageToView(request);
            ClientRequestSeqNumber++;

            // If no response is received after timeout, message is resent 
            int timeStep = 200; // ms
            List<Response> responses = null;
            do{
                for (int i = 0; i < DefaultTimeoutDuration; i += timeStep){
                    ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out responses);
                    if (responses != null && responses.Count >= 1){
                        return responses.First().Tuples.First();
                    }
                }
                // resend same request
                SendMessageToView(request);
            } while (responses.Count < 1);

        }

        public override Tuple Take(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);
            Tuple selectedTuple = null;
            SendMessageToView(request);
            ClientRequestSeqNumber++;

            int timeStep = 250; // ms
            for (int i = 0; i < DefaultTimeoutDuration; i += timeStep) {
                ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out var responses);
               
                if (responses.Count == View.Count) {
                    // get intersection of tuples from all lists received
                    List<Tuple> matchingTuples = new List<Tuple>();
                    foreach(Response response in responses) {
                        matchingTuples = (List<Tuple>) matchingTuples.Intersect(response.Tuples); // get common tuples in all response lists
                    }
                    
                    // select one tuple at random from intersection
                    if(matchingTuples.Count != 0) {
                        selectedTuple = matchingTuples.ElementAt((new Random()).Next(0, matchingTuples.Count));
                    }

                    // count the number of locks that were taken, if majority we can proceed to phase 2, if minority or timeout expired, redo take completely
                    for(int j = 0; j < DefaultTimeoutDuration; j += timeStep) {
                        ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out var storedResponses);
                        if(storedResponses.Count > View.Count / 2) {
                            // phase 2 - ask to remove the tuple when all acks have been received
                            RemoveTuple(selectedTuple);
                           
                            return selectedTuple;
                        }
                    }
                    // redo take and hope we get majority of locks
                    return Take(tuple);
                }
            }
            return null;
        }

        private void RemoveTuple(Tuple selectedTuple){
            Request requestForRemove = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.REMOVE, selectedTuple);
            ClientRequestSeqNumber++;

            int timeStep = 250;
            int ackCount;
            do {
                AckReceivedPerRequest.TryGetValue(requestForRemove.SeqNum, out ackCount);
                System.Threading.Thread.Sleep(timeStep);
            } while (ackCount < View.Count);
            
        }

        public override Message OnReceiveMessage(Message message) {
            if(message.GetType() == typeof(Ack)  ) {
                UpdateAckCounter((Ack) message);
                
            }
            // answer from read or take
            if(message.GetType() == typeof(Response)) {
                Response response = (Response) message;
                if (IsReadOrTakeResponse(response)){
                    ResponsesReceivedPerRequest.TryRemove(response.Request.SeqNum, out var storedResponses);
                    storedResponses.Add(response);
                    ResponsesReceivedPerRequest.TryAdd(response.Request.SeqNum, storedResponses);
                }
                
            }

            return null;
        }

        private bool IsReadOrTakeResponse(Response response){
            return response.Request.RequestType == RequestType.READ ||
                response.Request.RequestType == RequestType.TAKE;
        }


        private void UpdateAckCounter(Ack ack){
            if(ack.Message.GetType() == typeof(Request)) {
                Request request = (Request)ack.Message;
                if(request.RequestType == RequestType.READ) {
                    AckReceivedPerRequest.TryRemove(request.SeqNum, out var oldCounter);
                    AckReceivedPerRequest.TryAdd(request.SeqNum, ++oldCounter);
                }
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
