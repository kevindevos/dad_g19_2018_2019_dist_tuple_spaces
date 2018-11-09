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
        private ConcurrentDictionary<int, int> AckReceivedCounterPerRequest;

        // request seq number, responses
        private ConcurrentDictionary<int, List<Response>> ResponsesReceivedPerRequest;

        private ConcurrentDictionary<int, int> LocksTakenCountPerRequest;

        protected const int DefaultTimeoutDuration = 5;

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
            SendMessageDel = new SendMessageDelegate(SendMessageToView);
            AckReceivedCounterPerRequest = new ConcurrentDictionary<int, int>();
        }

        public ClientXL(string host, int port) : base(host, port) {
        }

        public override void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            SendMessageDel(request);
            ClientRequestSeqNumber++;

            int timeBetweenChecks = 200; // ms
            for (int i = 0; i < DefaultTimeoutDuration; i += timeBetweenChecks) {
                int acksReceivedSoFar;
                AckReceivedCounterPerRequest.TryGetValue(request.SeqNum, out acksReceivedSoFar);
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
            ClientRequestSeqNumber++;

            int timeBetweenChecks = 200; // ms
            for (int i = 0; i < DefaultTimeoutDuration; i += timeBetweenChecks) {
                List<Response> responses;
                ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out responses);
                if (responses.Count >= 1) {
                    return responses.First().Tuples.First();
                }
            }
            // failed timeout, redo read
            return Read(tuple);
        }

        public override Tuple Take(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);
            Tuple selectedTuple = null;
            SendMessageDel(request);
            ClientRequestSeqNumber++;

            int timeBetweenChecks = 250; // ms
            for (int i = 0; i < DefaultTimeoutDuration; i += timeBetweenChecks) {
                List<Response> responses;
                ResponsesReceivedPerRequest.TryGetValue(request.SeqNum, out responses);
                if (responses.Count == KnownServerRemotes.Count) {
                    List<Tuple> matchingTuples = new List<Tuple>();
                    foreach(Response response in responses) {
                        matchingTuples = (List<Tuple>) matchingTuples.Intersect(response.Tuples); // get common tuples in all response lists
                    }
                    // if there are any tuples common to all lists, select one randomly according to the algorithm
                    if(matchingTuples.Count != 0) {
                        selectedTuple = matchingTuples.ElementAt((new Random()).Next(0, matchingTuples.Count));
                    }

                    // count the number of locks that were taken, if majority we can proceed to phase 2, if minority or timeout expired, redo take completely
                    for(int j = 0; j < DefaultTimeoutDuration; j += timeBetweenChecks) {
                        int numAcceptedLocks;
                        LocksTakenCountPerRequest.TryGetValue(request.SeqNum, out numAcceptedLocks);
                        // if majority proceed to phase 2
                        if(numAcceptedLocks > KnownServerRemotes.Count / 2) {
                            //  PHASE 2
                            // proceed to multicast a Remove, and only return when all acks were received!
                            Request requestForRemove = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.REMOVE, selectedTuple);
                            ClientRequestSeqNumber++;

                            // return when all acks received
                            int ackCount;
                            do {
                                AckReceivedCounterPerRequest.TryGetValue(requestForRemove.SeqNum, out ackCount);
                                System.Threading.Thread.Sleep(timeBetweenChecks); // prevent CPU massacre
                                // TODO possible infinite loop here? for example if after removing a tuple, one of the servers dies we die of old age
                            } while (ackCount < KnownServerRemotes.Count);

                            // at this point the tuple should have been removed from all Replicas
                            return selectedTuple;

                        }
                    }
                    // redo take and hope we get majority of locks
                    return Take(tuple);
                }
            }


            return null;
        }

        public override Message OnReceiveMessage(Message message) {
            // if ack for read request, increment counter
            if(message.GetType() == typeof(Ack)  ) {
                Ack ack = (Ack)message;
                if(ack.Message.GetType() == typeof(Request)) {
                    Request request = (Request)ack.Message;
                    if(request.RequestType == RequestType.READ) {
                        int oldCounter;
                        AckReceivedCounterPerRequest.TryRemove(request.SeqNum, out oldCounter);
                        AckReceivedCounterPerRequest.TryAdd(request.SeqNum, ++oldCounter);
                    }
                }
            }
            // answer from read or take
            if(message.GetType() == typeof(Response)) {
                Response response = (Response)message;
                List<Response> storedResponses;
                ResponsesReceivedPerRequest.TryRemove(response.Request.SeqNum, out storedResponses);
                storedResponses.Add(response);
                ResponsesReceivedPerRequest.TryAdd(response.Request.SeqNum, storedResponses);
            }

            // TODO update numAcceptedLocks when a lock was accepted at server
            

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
