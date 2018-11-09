using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public abstract class Client : RemotingEndpoint {
        protected int ClientRequestSeqNumber;

        // <Request.sequenceNumber, Response>
        protected readonly Dictionary<int, Response> ReceivedResponses;

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

        public abstract void Write(Tuple tuple);
        public abstract Tuple Read(Tuple tuple);
        public abstract Tuple Take(Tuple tuple);

        


    }
}