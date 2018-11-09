using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace {
    public abstract class Server : RemotingEndpoint, ITupleOperations {
        public delegate Message SendMessageDelegate(Message message);

        // TODO move variables and logic from Server to ServerSMR that doesn't belong here

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> RequestList { get; }

        protected Server(string host, int port) : base(host, port, "Server") {
            Port = port;
            RequestList = new List<Request>();
        }

        protected Server() : this(DefaultServerHost, DefaultServerPort) { }

        private Server(int serverPort) : this(DefaultServerHost, serverPort) { }

        public Response PerformRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            switch (request.RequestType) {
                case RequestType.READ:
                    var resultTuples = Read(tupleSchema);
                    return new Response(request, resultTuples, EndpointURL);

                case RequestType.WRITE:
                    Write(request.Tuple);
                    return null;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Take(tupleSchema);
                    return new Response(request, resultTuples, EndpointURL);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SaveRequest(Request request) {
            lock (RequestList) {
                RequestList.Add(request);
            }
        }

        public void DeleteRequest(Request request) {
            lock (RequestList) {
                RequestList.Remove(request);
            }
        }

        public virtual void Log(string text) {
            Console.WriteLine("[SERVER:"+EndpointURL +"]   " + text);
        }

        public abstract void Write(Tuple tuple);
        public abstract List<Tuple> Read(TupleSchema tupleSchema);
        public abstract List<Tuple> Take(TupleSchema tupleSchema);
    }

}