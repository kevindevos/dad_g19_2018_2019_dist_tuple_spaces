using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public delegate Message SendMessageDelegate(Message message);

    public abstract class Client : RemotingEndpoint {
        protected int ClientRequestSeqNumber;
        // <Request.sequenceNumber, Response>
        protected readonly Dictionary<int, Response> ReceivedResponses;
        protected SendMessageDelegate SendMessageDel;

        // <Request.sequenceNumber, Semaphore>
        protected readonly Dictionary<int, SemaphoreSlim> RequestSemaphore;

        public Client() : this(DefaultClientHost, DefaultClientPort) { }
        
        public Client(string host, int port) : base(host, port, "Client") {
            ReceivedResponses = new Dictionary<int, Response>();
            ClientRequestSeqNumber = 0;
            RequestSemaphore = new Dictionary<int, SemaphoreSlim>();
        }

        public override Message OnReceiveMessage(Message message) {
            if (message.GetType() != typeof(Response)) throw new NotImplementedException();

            var response = (Response)message;

            lock (ReceivedResponses) {
                // if duplicated response, ignore
                if (ReceivedResponses.ContainsKey(response.Request.SeqNum)) return null;

                ReceivedResponses.Add(response.Request.SeqNum, response);
            }

            // if it receives at least one response for a blocking request then unblock ( can only send one and then block, so it will always unblock properly? )
            if (response.Request.RequestType.Equals(RequestType.READ) ||
                response.Request.RequestType.Equals(RequestType.TAKE)) {
                if (RequestSemaphore.TryGetValue(response.Request.SeqNum, out var semaphore)) {
                    semaphore.Release();
                    return null;
                }

                throw new NotImplementedException("No semaphore was allocated.");
            }
            return null;
        }

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }

        public void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            SendMessageDel(request);
            ClientRequestSeqNumber++;
        }

        public Tuple Read(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);

            return SendBlockingRequest(request);
        }

        public Tuple Take(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);
            return SendBlockingRequest(request);
        }

        private Tuple SendBlockingRequest(Request request) {
            RequestSemaphore[request.SeqNum] = new SemaphoreSlim(0, 1);
            SendMessageDel(request);
            ClientRequestSeqNumber++;

            WaitForResponse(request.SeqNum);

            if (ReceivedResponses[request.SeqNum].Tuples != null && ReceivedResponses[request.SeqNum].Tuples.Count > 0) {
                return ReceivedResponses[request.SeqNum].Tuples.First();
            }
            else {
                return null;
            }
        }

        // Wait for response. Disposes and removes semaphore.
        private void WaitForResponse(int requestSeqNum) {
            RequestSemaphore[requestSeqNum].Wait();
            RequestSemaphore[requestSeqNum].Dispose();
            RequestSemaphore.Remove(requestSeqNum);
        }

    }
}