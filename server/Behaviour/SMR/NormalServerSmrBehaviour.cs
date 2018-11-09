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
            Server.Log("Received an order");
            lock (Server.LastExecutedOrders)
            {
                if (SequenceNumberIsNext(order)) {
                    Server.SavedOrders.Add(order);
                    Server.DeleteRequest(order.Request);
                    Server.LastOrderSequenceNumber = order.SeqNum;

                    Server.Log("Executing the order");
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

            // check periodically for answer
            int timeBetweenChecks = 50; // ms
            for(int i = 0; i < DefaultRequestToMasterAckTimeoutDuration; i+=timeBetweenChecks) {
                if(response != null && response.GetType() != typeof(Ack)) {
                    System.Threading.Thread.Sleep(i);
                }
                else {
                    Server.Log("Received an Ack");
                    return null;
                }
            }
            TriggerReelection();
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