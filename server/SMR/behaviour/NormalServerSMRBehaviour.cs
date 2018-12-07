using System;
using System.Linq;
using CommonTypes.message;

namespace ServerNamespace.SMR.Behaviour {
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
                if (CanExecuteOrder(order)) {
                    Server.SavedOrders.Add(order);
                    Server.DeleteRequest(order.Request);
                    Server.LastOrderSequenceNumber = order.SeqNum;

                    Server.UpdateLastExecutedOrder(order);
                    Server.UpdateLastExecutedRequest(order.Request);

                    Server.Log("Executing the order");
                    return Server.ProcessRequest(order.Request);
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

        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
            SendMessageToMaster(request);
        }

        public override void ProcessAskOrder(AskOrder askOrder) {
        }

        private void SendMessageToMaster(Message request)
        {
            while (true)
            {
                var masterUrl = "";
                lock (Server.View)
                {
                    masterUrl = Server.View.Nodes.OrderBy(s => s).First();
                }
                Server.Log("Sending message to master (" + masterUrl + ")");

                try
                {
                    Server.SingleCastMessage(masterUrl, request);
                    Server.WaitMessage(request);
                    break;
                }
                catch (Exception e)
                {
                    Server.Log("Couldn't deliver message to master (" + masterUrl + ")");
                    // try again, until it receives an ok
                }
            }
        }
    }
}