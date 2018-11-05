using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class NormalServerSmrBehaviour : ServerBehaviour
    {
        public NormalServerSmrBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message ProcessMessage(Message message) {
            if (message.GetType().Equals(typeof(Order))) {
                Order order = (Order)message;
                Server.DeleteRequest(order.Request);
                Server.LastOrderSequenceNumber = order.OrderSeqNumber;

                return PerformRequest(order.Request);
            }
            else if (message.GetType().Equals(typeof(Request))) {
                Request request = (Request)message;
                Server.SaveRequest(request);

                // TODO Problem, when client does read or take, it is blocking, it expects a return message, but the server needs to wait for the order of the master, ?!?!?

            }
            return null;
        }
    }
}