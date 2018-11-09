using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientXL : Client {
        // request seq number, counter
        private ConcurrentDictionary<int, int> ackReceivedCounterPerRequest;

        // request seq number, responses
        private ConcurrentDictionary<int, List<Response>> responsesReceivedPerRequest;

        protected const int DefaultTimeoutForReadAcks = 5;

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
            SendMessageDel = new SendMessageDelegate(SendMessageToView);
            ackReceivedCounterPerRequest = new ConcurrentDictionary<int, int>();
        }

        public ClientXL(string host, int port) : base(host, port) {
        }

        public override void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            SendMessageDel(request);
            ClientRequestSeqNumber++;

            int timeBetweenChecks = 200; // ms
            for (int i = 0; i < DefaultTimeoutForReadAcks; i += timeBetweenChecks) {
                int acksReceivedSoFar;
                ackReceivedCounterPerRequest.TryGetValue(request.SeqNum, out acksReceivedSoFar);
                if(acksReceivedSoFar >= KnownServerRemotes.Count) {
                    return;
                }
            }
            // timeout reached, redo write
            Write(tuple);
        }

        public override Tuple Read(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);
            SendMessageDel(request);

            int timeBetweenChecks = 200; // ms
            for (int i = 0; i < DefaultTimeoutForReadAcks; i += timeBetweenChecks) {
                List<Response> responses;
                responsesReceivedPerRequest.TryGetValue(request.SeqNum, out responses);
                if (responses.Count >= 1) {
                    return responses.First().Tuples.First();
                }
            }
            // failed timeout, redo read
            Read(tuple);

            return null;
        }

        public override Tuple Take(Tuple tuple) {
            // TODO
        }

        public override Message OnReceiveMessage(Message message) {
            // if ack for read request, increment counter
            if(message.GetType() == typeof(Ack)  ) {
                Ack ack = (Ack)message;
                if(ack.Message.GetType() == typeof(Request)) {
                    Request request = (Request)ack.Message;
                    if(request.RequestType == RequestType.READ) {
                        int oldCounter;
                        ackReceivedCounterPerRequest.TryRemove(request.SeqNum, out oldCounter);
                        ackReceivedCounterPerRequest.TryAdd(request.SeqNum, ++oldCounter);
                    }
                }
            }
            // answer from read or take
            if(message.GetType() == typeof(Response)) {
                Response response = (Response)message;
                if(response.Request.RequestType == RequestType.READ) {
                    List<Response> storedResponses;
                    responsesReceivedPerRequest.TryRemove(response.Request.SeqNum, out storedResponses);
                    storedResponses.Add(response);
                    responsesReceivedPerRequest.TryAdd(response.Request.SeqNum, storedResponses);
                }

                if (response.Request.RequestType == RequestType.TAKE) {
                    // TODO
                }

                throw new NotImplementedException();
            }


            return null;
        }



        private Message SendMessageToView(Message message) {
            SendMessageToRemotes(KnownServerRemotes, message);
            return null;
        }

        

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
