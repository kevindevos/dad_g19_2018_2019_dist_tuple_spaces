using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientSMR : Client{
        public ClientSMR(string host, int port) {
        }

        public ClientSMR() :base(DefaultClientHost, DefaultClientPort){
        }

        public override void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
            SendMessageToRandomServer(request);
            ClientRequestSeqNumber++;
        }

        public override Tuple Read(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);

            RequestSemaphore[request.SeqNum] = new SemaphoreSlim(0, 1);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
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

        public override Tuple Take(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);

            RequestSemaphore[request.SeqNum] = new SemaphoreSlim(0, 1);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
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

       

        
    }
}
