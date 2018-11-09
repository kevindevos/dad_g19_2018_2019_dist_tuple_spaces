using System.Linq;
using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class MasterServerSMRBehaviour : ServerSMRBehaviour {
        
        public MasterServerSMRBehaviour(ServerSMR server) : base(server)
        {
        }

        public override Message ProcessRequest(Request request) {
            Server.SaveRequest(request);
            while(Decide(request.SrcEndpointURL));

            // create an ack that will be sent back to confirm the request was received
            Server.Log("Sending back an Ack");
            Ack ack = new Ack(Server.EndpointURL, request);

            return ack;
        }

        public override Message ProcessOrder(Order order) {
            // for when a master sends order, crashes and the order arrives after the election of a new master, act as a normal server executing an order
            Server.SavedOrders.Add(order);
            Server.DeleteRequest(order.Request);
            Server.LastOrderSequenceNumber = order.SeqNum;

            return PerformRequest(order.Request);
        }
        
        // Check if there are requests with ANY client sequence number that is valid for execution 
        // Return true, if it executed a request, false otherwise
        public bool Decide() {
            lock (Server.RequestList) {
                foreach (var request in Server.RequestList)
                {
                    if (!SequenceNumberIsNext(request)) continue;
                    
                    Order order = new Order(request, Server.LastOrderSequenceNumber++, Server.EndpointURL);
                    Server.RequestList.Remove(request);
                    Server.SendMessageToKnownServers(order);
                    Server.SavedOrders.Add(order);

                    return true;
                }
            }
            return false;
        }
        
        // Check if there are requests with the same endpointURL (client identifier) that can be executed
        // Return true, if it executed a request, false otherwise
        private bool Decide(string endpointURL) {
            lock (Server.RequestList) {
                foreach (var request in Server.RequestList)
                {
                    if (!request.SrcEndpointURL.Equals(endpointURL) || (!SequenceNumberIsNext(request))) continue;
                    
                    Order order = new Order(request, Server.LastOrderSequenceNumber++, Server.EndpointURL);
                    Server.RequestList.Remove(request);
                    Server.SendMessageToKnownServers(order);
                    Server.SavedOrders.Add(order);

                    return true;
                }
            }
            return false;
        }

        

        // A Normal server sent us an AskOrder, so master needs to find the order in recently SavedOrders and resend it.
        // TODO Does SavedOrders need to be synchronized?
        // TODO should receive and send a List
        public override void ProcessAskOrder(AskOrder askOrder) {
            foreach (var order in Server.SavedOrders)
            {
                if(order.SeqNum == askOrder.WantedSequenceNumber) {
                    Server.SendMessageToRemoteURL(askOrder.SrcRemoteURL, order);
                }
            }
        }
    }
}