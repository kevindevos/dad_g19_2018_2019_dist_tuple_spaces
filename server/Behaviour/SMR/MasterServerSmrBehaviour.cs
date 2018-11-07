using System;
using System.Linq;
using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class MasterServerSMRBehaviour : ServerSMRBehaviour {
        
        public MasterServerSMRBehaviour(Server server) : base(server)
        {
        }

        
        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
            Decide(request.SrcEndpointURL);
        }

        public override Message ProcessOrder(Order order) {
            // for when a master sends order, crashes and the order arrives after the election of a new master, act as a normal server executing an order
            Server.SavedOrders.Add(order);
            Server.DeleteRequest(order.Request);
            Server.LastOrderSequenceNumber = order.SeqNum;

            return PerformRequest(order.Request);
        }

        // Check if there are requests with ANY client sequence number that is valid for execution 
        public void Decide() {
            lock (Server.RequestList) {
                for (int i = 0; i < Server.RequestList.Count; i++) {
                    Request request = Server.RequestList.ElementAt(i);

                    if (SequenceNumberIsNext(request)) {
                        Order order = new Order(request, Server.LastOrderSequenceNumber+1,Server.endpointURL);
                        Server.RequestList.Remove(request);
                        BroadcastOrder(order);
                        Server.SavedOrders.Add(order);
                        Decide(); // check again if there are more
                        return;
                    }

                }
            }
        }

        // Check if there are requests with the same endpointURL (client identifier) that can be executed
        public void Decide(string endpointURL) {
            lock (Server.RequestList) {

                for (int i = 0; i < Server.RequestList.Count; i++) {
                    Request request = Server.RequestList.ElementAt(i);

                    if (request.SrcEndpointURL.Equals(endpointURL) && (SequenceNumberIsNext(request))) {
                        Order order = new Order(request, Server.LastOrderSequenceNumber+1, Server.endpointURL);
                        Server.RequestList.Remove(request);
                        BroadcastOrder(order);
                        Server.SavedOrders.Add(order);
                        Decide(endpointURL); // check again if there are more
                        return;
                    }

                }
            }
        }

        // Send an order to all servers
        public void BroadcastOrder(Order order) {
            Server.SendMessageToKnownServers(order);
            ++Server.LastOrderSequenceNumber;
        }

        // A Normal server sent us an AskOrder, so master needs to find the order in recently SavedOrders and resend it.
        public override void ProcessAskOrder(AskOrder askOrder) {
            for(int i = 0; i < Server.SavedOrders.Count; i++) {
                if(Server.SavedOrders.ElementAt(i).SeqNum == askOrder.WantedSequenceNumber) {
                    Server.SendMessateToRemoteURL(askOrder.SrcRemoteURL, Server.SavedOrders.ElementAt(i));
                }
            }
        }
    }
}