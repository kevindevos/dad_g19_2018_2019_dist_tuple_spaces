using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class NormalServerSMRBehaviour : ServerSMRBehaviour
    {
        public NormalServerSMRBehaviour(Server server) : base(server)
        {
        }

        public override Message ProcessOrder(Order order) {
            if (SequenceNumberIsNext(order)) {
                Server.SavedOrders.Add(order);
                Server.DeleteRequest(order.Request);
                Server.LastOrderSequenceNumber = order.SeqNum;

                return PerformRequest(order.Request);
            }

            // TODO special case, what if a normal server crashes, and the last order sequence number is reset to 0, 
            // and it receives order nr 1994 , does it ask for 1993 orders even without knowing how many were executed ( if there's no persistency, we need to execute all again), causing data inconsistency?
            AskForMissingOrders(Server.LastOrderSequenceNumber + 1, order.SeqNum - 1);
            return null;

        }

        // ask the master to send back the missing orders with sequence numbers from startSeqNum up to endSeqNum, inclusive
        //TODO ask for all of them in one request
        private void AskForMissingOrders(int startSeqNum, int endSeqNum) {
            for (int i = startSeqNum; i <= endSeqNum; i++) {
                AskForMissingOrder(i);
            }
        }

        // ask the master to send back missing order with sequence number i
        //TODO ask for all of them in one request
        private void AskForMissingOrder(int wantedOrderSequenceNumber) {
            AskOrder askOrder = new AskOrder(Server.endpointURL, wantedOrderSequenceNumber);
            Server.SendMessageToKnownServers(askOrder);
        }

        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
        }

        public override void ProcessAskOrder(AskOrder askOrder) {
        }
    }
}