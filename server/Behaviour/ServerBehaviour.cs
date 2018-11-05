using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace.Behaviour {
    public abstract class ServerBehaviour {
        protected readonly Server.Server Server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server.Server server) {
            Server = server;
        }

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

        public abstract Message ProcessMessage(Message message);

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool ClientRequestSequenceNumberIsValid(Request request) {
            Request lastExecutedRequest;
            return Server.LastExecutedClientRequests.TryGetValue(request.SrcRemoteURL, out lastExecutedRequest) && lastExecutedRequest.SeqNum == request.SeqNum - 1;
        }


    }
}
