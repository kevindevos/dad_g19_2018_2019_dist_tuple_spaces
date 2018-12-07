using CommonTypes.message;

namespace ServerNamespace.SMR.Behaviour
{
    public class MasterServerSMRBehaviour : ServerSMRBehaviour {
        
        public MasterServerSMRBehaviour(ServerSMR server) : base(server)
        {
        }

        public override void ProcessRequest(Request request) {
            Server.SaveRequest(request);
            while(Decide(request.SrcRemoteURL));
        }

        public override Message ProcessOrder(Order order) {
            // for when a master sends order, crashes and the order arrives after the election of a new master, act as a normal server executing an order
            Server.SavedOrders.Add(order);
            Server.DeleteRequest(order.Request);
            Server.LastOrderSequenceNumber = order.SeqNum;

            return Server.ProcessRequest(order.Request);
        }
        
        // Check if there are requests with ANY client sequence number that is valid for execution 
        // Return true, if it executed a request, false otherwise
        public bool Decide() {
            lock (Server.RequestList) {
                foreach (var request in Server.RequestList)
                {
                    if (!CanExecuteRequest(request)) {
                        Server.Log("Can't execute request with sequence number: " + request.SeqNum);
                        continue;
                    }
                    OrderRequestForExecution(request);

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
                    if (!request.SrcRemoteURL.Equals(endpointURL) || (!CanExecuteRequest(request))) {
                        Server.Log("Can't execute request with sequence number: " + request.SeqNum);
                        Server.Log("request remoteurl : " + request.SrcRemoteURL);
                        Server.Log("given remoteurl : " + endpointURL);
                        continue;
                    }
                    OrderRequestForExecution(request);
                    
                    return true;
                }
            }
            return false;
        }

       

        private void OrderRequestForExecution(Request request) {
            Order order = new Order(request, Server.LastOrderSequenceNumber++, Server.EndpointURL);
            Server.RequestList.Remove(request);
            Server.Log("Sending Order to all servers.");
            
            Server.MulticastMessageWaitAll(Server.View.Nodes, order);
            Server.WaitMessage(order, Server.View.Nodes);
            
            Server.SavedOrders.Add(order);

            Server.UpdateLastExecutedOrder(order);
            Server.UpdateLastExecutedRequest(order.Request);

            // Process request locally
            Message response = Server.ProcessRequest(order.Request);  
            
            // if read or take answer to client
            if(order.Request.GetType() == typeof(ReadRequest) || order.Request.GetType() == typeof(TakeRequest)) {
                Server.Log("Sending back message to client with response: " + response);
                Server.SingleCastMessage(request.SrcRemoteURL, response);
                Server.WaitMessage(response);
            }
            
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