using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class Client : RemotingEndpoint {
        private int _clientRequestSeqNumber;

        // <Request.sequenceNumber, Response>
        private readonly Dictionary<int, Response> _receivedResponses;
        // <Request.sequenceNumber, Semaphore>
        private readonly Dictionary<int, SemaphoreSlim> _requestSemaphore; 
        
        public Client() : this(DefaultClientHost, DefaultClientPort) { }
        
        public Client(string host, int port) : base(host, port, "Client") {
            _receivedResponses = new Dictionary<int, Response>();
            _requestSemaphore = new Dictionary<int, SemaphoreSlim>();
            _clientRequestSeqNumber = 0;
        }

        public void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(_clientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
            SendMessageToRandomServer(request);
            _clientRequestSeqNumber++;
        }

        public Tuple Read(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(_clientRequestSeqNumber, EndpointURL, RequestType.READ, tuple);
            
            _requestSemaphore[request.SeqNum] = new SemaphoreSlim(0,1);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
            SendMessageToRandomServer(request);
            _clientRequestSeqNumber++;

            
            WaitForResponse(request.SeqNum);
            
            
            if(_receivedResponses[request.SeqNum].Tuples != null && _receivedResponses[request.SeqNum].Tuples.Count > 0) {
                return _receivedResponses[request.SeqNum].Tuples.First();
            }
            else {
                return null;
            }
        }

        public Tuple Take(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(_clientRequestSeqNumber, EndpointURL, RequestType.TAKE, tuple);

            _requestSemaphore[request.SeqNum] = new SemaphoreSlim(0,1);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
            SendMessageToRandomServer(request);
            _clientRequestSeqNumber++;

            WaitForResponse(request.SeqNum);

            if (_receivedResponses[request.SeqNum].Tuples != null && _receivedResponses[request.SeqNum].Tuples.Count > 0) {
                return _receivedResponses[request.SeqNum].Tuples.First();
            }
            else {
                return null;
            }
        }



        public override Message OnReceiveMessage(Message message) {
            
            if (message.GetType() != typeof(Response)) throw new NotImplementedException();
            
            var response = (Response)message;

            lock (_receivedResponses)
            {
                // if duplicated response, ignore
                if (_receivedResponses.ContainsKey(response.Request.SeqNum)) return null;
                
                _receivedResponses.Add(response.Request.SeqNum, response);
            }
            
            
            // if it receives at least one response for a blocking request then unblock ( can only send one and then block, so it will always unblock properly? )
            if (response.Request.RequestType.Equals(RequestType.READ) ||
                response.Request.RequestType.Equals(RequestType.TAKE))
            {
                if (_requestSemaphore.TryGetValue(response.Request.SeqNum, out var semaphore))
                {
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
        
        // Wait for response. Disposes and removes semaphore.
        private void WaitForResponse(int requestSeqNum)
        {
            _requestSemaphore[requestSeqNum].Wait();
            _requestSemaphore[requestSeqNum].Dispose();
            _requestSemaphore.Remove(requestSeqNum);
        }
    }
}