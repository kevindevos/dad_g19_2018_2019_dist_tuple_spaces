using CommonTypes;
using CommonTypes.message;
using System;
using System.Linq;

namespace ServerNamespace.Behaviour.SMR
{
    public class NormalServerSMRBehaviour : ServerSMRBehaviour
    {
        public NormalServerSMRBehaviour(ServerSMR server) : base(server)
        {
        }

        // shorten the locks
        public override Message ProcessOrder(Order order) {
            lock (Server.LastExecutedOrders)
            {
                if (SequenceNumberIsNext(order)) {
                    Server.SavedOrders.Add(order);
                    Server.DeleteRequest(order.Request);
                    Server.LastOrderSequenceNumber = order.SeqNum;

                    return PerformRequest(order.Request);
                }

                AskForMissingOrders(Server.LastOrderSequenceNumber + 1, order.SeqNum - 1);
                return null;
            }
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
            AskOrder askOrder = new AskOrder(Server.EndpointURL, wantedOrderSequenceNumber);
            SendMessageToMaster(askOrder);
        }

        public override Message ProcessRequest(Request request) {
            Server.SaveRequest(request);
            Server.Log("Sending request to Master");

            Message response = SendMessageToMaster(request);

            // wait a few seconds and then check whether or not an Ack was received 
            Server.Log("NET Remoting Thread Sleeping now");
            System.Threading.Thread.Sleep(DefaultRequestToMasterAckTimeoutDuration*1000);
            Server.Log("Waking up");
            if (response != null && response.GetType() != typeof(Ack) ) {
                Server.Log("Did not receive Ack, triggering new election");
                TriggerReelection();
            }
            else {
                Server.Log("Received an Ack");
            }
            return null;
        }

        private void TriggerReelection() {
            // TODO
            throw new NotImplementedException();
        }

        public override void ProcessAskOrder(AskOrder askOrder) {
        }

        private Message SendMessageToMaster(Message request) {
            if(Server.MasterEndpointURL != null) {
                return Server.SendMessageToRemoteURL(Server.MasterEndpointURL, request);
            }
            else {
                // TODO master is not known, problem?
                return null;
            }
        }
    }
}