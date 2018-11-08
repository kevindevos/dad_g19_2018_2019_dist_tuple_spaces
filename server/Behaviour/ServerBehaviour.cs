using System;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;

namespace ServerNamespace.Behaviour {
    public abstract class ServerBehaviour {
        protected readonly Server Server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            Server = server;
        }

        public abstract Message ProcessMessage(Message message);

        public Response PerformRequest(Request request) {
            // remove from the requestList
            Server.DeleteRequest(request);

            var tupleSchema = new TupleSchema(request.Tuple);
            switch (request.RequestType) {
                case RequestType.READ:
                    var resultTuples = Server.Read(tupleSchema);
                    return new Response(request, resultTuples, Server.endpointURL);

                case RequestType.WRITE:
                    Server.Write(request.Tuple);
                    return null;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Server.Take(tupleSchema);
                    return new Response(request, resultTuples, Server.endpointURL);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        

    }
}
