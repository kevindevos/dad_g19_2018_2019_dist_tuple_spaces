using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;
using System.Linq;

namespace ServerNamespace.Behaviour.SMR {
    public abstract class ServerSMRBehaviour : ServerBehaviour{
        protected new readonly ServerSMR Server;

        protected const int DefaultRequestToMasterAckTimeoutDuration = 5; // seconds

        public ServerSMRBehaviour(ServerSMR server) : base(server) {
            Server = server;
        }

        public abstract Message ProcessRequest(Request request);
        public abstract Message ProcessOrder(Order order);
        public abstract void ProcessAskOrder(AskOrder askOrder);

        public override Message ProcessMessage(Message message) {
            if (message.GetType() == typeof(Request)) {
                return ProcessRequest((Request)message);
            }

            if (message.GetType() == typeof(Order)) {
                return ProcessOrder((Order)message);
            }
            
            // if an Elect message, define new master as the one included in the Elect message
            if (message.GetType() == typeof(Elect)){
                Server.MasterEndpointURL = ((Elect)message).NewMasterURL;
            }
            
            throw new NotImplementedException();
        }

        public override Response PerformRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);
            switch (request.RequestType) {
                case RequestType.READ:
                    var resultTuples = Server.Read(tupleSchema);
                    return new Response(request, resultTuples, Server.EndpointURL);

                case RequestType.WRITE:
                    Server.Write(request.Tuple);
                    return null;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Server.Take(tupleSchema);
                    return new Response(request, resultTuples, Server.EndpointURL);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        protected bool SequenceNumberIsNext(Request request) {
            var lastRequest = Server.LastExecutedRequests.GetOrAdd(request.SrcRemoteURL, request);
            return lastRequest.SeqNum + 1 == request.SeqNum || request.SeqNum == 0;
        }

        // check if the sequence number of this order is just 1 higher than the previous ( else there is a missing order )
        protected bool SequenceNumberIsNext(Order order) {
            var lastOrder = Server.LastExecutedOrders.GetOrAdd(order.SrcRemoteURL, order);
            return lastOrder.SeqNum + 1 == order.SeqNum || order.SeqNum == 0;
        }
    }
}