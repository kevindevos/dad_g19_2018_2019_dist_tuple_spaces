using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;
using System.Linq;

namespace ServerNamespace.SMR.Behaviour {
    public abstract class ServerSMRBehaviour {
        protected readonly ServerSMR Server;

        protected const int DefaultRequestToMasterAckTimeoutDuration = 5; // seconds

        public ServerSMRBehaviour(ServerSMR server) {
            Server = server;
        }

        public abstract Message ProcessOrder(Order order);
        public abstract void ProcessAskOrder(AskOrder askOrder);
        public abstract void ProcessRequest(Request request);

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        protected bool CanExecuteRequest(Request request) {
            var lastRequest = Server.LastExecutedRequests.GetOrAdd(request.SrcRemoteURL, request);
            return lastRequest.SeqNum + 1 == request.SeqNum || request.SeqNum == 0;
        }

        // check if the sequence number of this order is just 1 higher than the previous ( else there is a missing order )
        protected bool CanExecuteOrder(Order order) {
            var lastOrder = Server.LastExecutedOrders.GetOrAdd(order.SrcRemoteURL, order);
            return lastOrder.SeqNum + 1 == order.SeqNum || order.SeqNum == 0;
        }

        

    }
}