using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace.Behaviour {
    public abstract class ServerBehaviour {
        protected readonly Server Server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            Server = server;
        }

        public abstract Message ProcessRequest(Request request);
        public abstract Message ProcessOrder(Order order);

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

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool SequenceNumberIsNext(Request request) {
            Request lastExecutedRequest;
            return Server.LastExecutedRequests.TryGetValue(request.SrcRemoteURL, out lastExecutedRequest) && lastExecutedRequest.SeqNum == request.SeqNum - 1;
        }
      
        // check if the sequence number of this order is just 1 higher than the previous ( else there is a missing order )
        public bool SequenceNumberIsNext(Order order) {
            Order lastExecutedOrder;
            return Server.LastExecutedOrders.TryGetValue(order.SrcRemoteURL, out lastExecutedOrder) && lastExecutedOrder.SeqNum == order.SeqNum - 1;
        }

    }
}
