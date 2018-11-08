using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR {
    public abstract class ServerSMRBehaviour : ServerBehaviour{

        public ServerSMRBehaviour(Server server) : base(server) { }

        public abstract void ProcessRequest(Request request);
        public abstract Message ProcessOrder(Order order);
        public abstract void ProcessAskOrder(AskOrder askOrder);

        public override Message ProcessMessage(Message message) {
            if (message.GetType() == typeof(Request)) {
                ProcessRequest((Request)message);
            }

            if (message.GetType() == typeof(Order)) {
                return ProcessOrder((Order)message);
            }

            return null;
        }

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool SequenceNumberIsNext(Request request) {
            return request.SeqNum == 0 || Server.LastExecutedRequests.GetOrAdd(request.SrcRemoteURL, request).SeqNum == request.SeqNum - 1 ||
                   request.SeqNum == 0; //TODO do this better
        }

        // check if the sequence number of this order is just 1 higher than the previous ( else there is a missing order )
        public bool SequenceNumberIsNext(Order order) {
            return order.SeqNum == 0 || Server.LastExecutedOrders.GetOrAdd(order.SrcRemoteURL, order).SeqNum == order.SeqNum - 1 ||
                   order.SeqNum == 0; //TODO do this better
        }
    }
}
