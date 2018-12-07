using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientSMR : Client{
        
        public ClientSMR() :this(DefaultClientHost, DefaultClientPort){
        }
        
        public ClientSMR(string host, int port) : this(BuildRemoteUrl(host, port, ClientObjName)){
        }

        public ClientSMR(string remoteUrl, IEnumerable<string> knownServerUrls = null) : base(remoteUrl, knownServerUrls){
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
            if (response.Request.GetType() == typeof(ReadRequest) ||
                response.Request.GetType() == typeof(TakeRequest)) {
                if (RequestSemaphore.TryGetValue(response.Request.SeqNum, out var semaphore)) {
                    semaphore.Release();
                    return null;
                }

                throw new NotImplementedException("No semaphore was allocated.");
            }
            return null;
        }

        
        public override void Write(Tuple tuple) {
            var request = new WriteRequest(ClientRequestSeqNumber, EndpointURL, tuple);

            Log("[SEQ:" + ClientRequestSeqNumber + "] Write: " + tuple);
            SendMessageToRandomServer(request);
            ClientRequestSeqNumber++;
        }

        public override Tuple Read(Tuple tuple) {
            var request = new ReadRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            Log("[SEQ:" + ClientRequestSeqNumber + "] Read: " + tuple);
            return SendBlockingRequest(request);
        }

        public override Tuple Take(Tuple tuple) {
            var request = new TakeRequest(ClientRequestSeqNumber, EndpointURL, tuple);
            Log("[SEQ:"+ClientRequestSeqNumber+"] Take: " + tuple);
            return SendBlockingRequest(request);
        }

        private Tuple SendBlockingRequest(Request request) {
            RequestSemaphore[request.SeqNum] = new SemaphoreSlim(0, 1);
            SendMessageToRandomServer(request);
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


        // tries to send a message to a random server
        // if, 5 seconds, exception or viewChange? try another server
        private void SendMessageToRandomServer(Message message) {
            while (true)
            {
                var randomServer = "";
                var random = new Random();
                lock (View)
                {
                    var i = random.Next(0, View.Size());
                    randomServer = View.Nodes.ToArray()[i];
                }

                try
                {
                    SingleCastMessage(randomServer, message);
                    WaitMessage(message);
                    break;
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
    }
}
