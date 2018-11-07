using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class NormalServerSMRBehaviour : ServerBehaviour
    {
        public NormalServerSMRBehaviour(Server server) : base(server)
        {
        }

        public override Message ProcessOrder(Order order) {
            Server.DeleteRequest(order.Request);
            Server.LastOrderSequenceNumber = order.SeqNum;

            return PerformRequest(order.Request);
        }

        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
        }
    }
}