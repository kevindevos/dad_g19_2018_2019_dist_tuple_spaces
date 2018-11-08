using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR {
    public abstract class ServerSMRBehaviour : ServerBehaviour{
        protected new ServerSMR Server;

        protected int DEFAULT_REQUEST_TO_MASTER_ACK_TIMEOUT_DURATION = 5; // seconds

        public ServerSMRBehaviour(ServerSMR server) : base(server) {
            Server = server;
        }

        public abstract Message ProcessRequest(Request request);
        public abstract Message ProcessOrder(Order order);
        public abstract void ProcessAskOrder(AskOrder askOrder);

        public override Message ProcessMessage(Message message) {
            // if an Elect message, define new master as the one included in the Elect message
            if (message.GetType().Equals(typeof(Elect))){
                Server.MasterEndpointURL = ((Elect)message).NewMasterURL;
            }

            if (message.GetType().Equals(typeof(Request))) {
                return ProcessRequest((Request)message);
            }

            if (message.GetType().Equals(typeof(Order))) {
                return ProcessOrder((Order)message);
            }

            return null;
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
