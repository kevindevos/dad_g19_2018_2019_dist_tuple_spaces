using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientSMR : Client{
        
        public ClientSMR() :this(DefaultClientHost, DefaultClientPort){
        }
        
        public ClientSMR(string host, int port) : this(BuildRemoteUrl(host, port, ClientObjName)){
        }

        public ClientSMR(string remoteUrl, List<string> knownServerUrls) : base(remoteUrl, knownServerUrls){
        }

        public ClientSMR(string remoteUrl) : base(remoteUrl){
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

        
        public override void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            Log("[SEQ:" + ClientRequestSeqNumber + "] Write: " + tuple.ToString());
            SendMessageToRandomServer(request);
            ClientRequestSeqNumber++;
        }

        public override Tuple Read(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);
            Log("[SEQ:" + ClientRequestSeqNumber + "] Read: " + tuple.ToString());
            return SendBlockingRequest(request);
        }

        public override Tuple Take(Tuple tuple) {
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);
            Log("[SEQ:"+ClientRequestSeqNumber+"] Take: " + tuple.ToString());
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

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
