using System;
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
            else {
                AskForMissingOrders(Server.LastOrderSequenceNumber + 1, order.SeqNum - 1);
                return null;
            }
        }

        // ask the master to send back the missing orders with sequence numbers from startSeqNum up to endSeqNum, inclusive
        private void AskForMissingOrders(int startSeqNum, int endSeqNum) {
            for (int i = startSeqNum; i <= endSeqNum; i++) {
                AskForMissingOrder(i);
            }
        }

        // ask the master to send back missing order with sequence number i
        private void AskForMissingOrder(int wantedOrderSequenceNumber) {
            AskOrder askOrder = new AskOrder(Server.endpointURL, wantedOrderSequenceNumber);
            Server.SendMessageToKnownServers(askOrder);
        }

        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
        }

        public override void ProcessAskOrder(AskOrder askOrder) {
            return; // Normal server does nothing, only master takes care of it, reduces message flooding
        }
    }
}