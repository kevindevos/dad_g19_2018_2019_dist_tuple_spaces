using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes.message;
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

        public ClientXL(string remoteUrl, IEnumerable<string> knownServerUrls = null) : base(remoteUrl, knownServerUrls) {
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

            // wait until it gets a any message 
            WaitMessage(request, View.Nodes);

            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryGetValue(request, out var replyResult);
                resultMessage = replyResult?.GetAnyResult();
            }

            ClientRequestSeqNumber++;
            Response response = (Response) resultMessage;
            
            return response.Tuples.First();
        }

        public override Tuple Take(Tuple tuple) {
            int timeStep = 250; // ms
            List<Message> messages = null;
            Message resultMessage = null;
            List<Tuple> intersection = new List<Tuple>();
            Tuple selectedTuple = new Tuple(new List<object>());
            
            // PHASE 1
            var takeRequest = new TakeRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            
            // wait for all responses
            MulticastMessageWaitAll(View.Nodes, takeRequest);
            WaitMessage(takeRequest, View.Nodes);
            ClientRequestSeqNumber++;
            
            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryGetValue(takeRequest, out var replyResult);
                messages = new List<Message>(replyResult?.GetAllResults());
            }
          
            // get the intersection
            Response response;
            intersection = ((Response) messages.First()).Tuples;
            foreach (Message msg in messages){
                response = (Response) msg;
                intersection = new List<Tuple>(intersection.Intersect(response.Tuples));
            }
            
            // choose random tuple from intersection
            selectedTuple = intersection.ElementAt((new Random()).Next(0, intersection.Count));
            
            // phase 2, issue a take remove and wait for all acks
            Message takeRemove = new TakeRemove(EndpointURL, selectedTuple, takeRequest.SeqNum);
            MulticastMessageWaitAll(View.Nodes, takeRemove);
            WaitMessage(takeRemove, View.Nodes);

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
