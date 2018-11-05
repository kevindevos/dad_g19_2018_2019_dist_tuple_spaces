using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace.Behaviour {
    public abstract class ServerBehaviour : IServerBehaviour {
        protected readonly Server.Server Server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        protected ServerBehaviour(Server.Server server) {
            Server = server;
        }

        public abstract Message OnReceiveMessage(Message message);
        public abstract Message OnSendMessage(Message message);


        // TODO check for concurrency
        protected Message PerformRequest(Request request) {
            // remove from the requestList
            Server.RemoveRequestFromList(request);

            var tupleSchema = new TupleSchema(request.tuple);
            switch (request.requestType) {
                case RequestType.READ:
                    var resultTuples = Server.Read(tupleSchema);
                    return new Response(request.seqNum, request.clientRemoteURL, resultTuples);

                case RequestType.WRITE:
                    Server.Write(request.tuple);
                    return null;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.tuple);
                    resultTuples = Server.Take(tupleSchema);
                    return new Response(request.seqNum, request.clientRemoteURL, resultTuples);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}
