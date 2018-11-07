using System;
using System.Linq;
using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class MasterServerSMRBehaviour : ServerBehaviour {
        
        public MasterServerSMRBehaviour(Server server) : base(server)
        {
        }

         // Check if there are requests with ANY client sequence number that is valid for execution 
         public void Decide() {
            lock (Server.RequestList) {
                for (int i = 0; i < Server.RequestList.Capacity; i++) {
                    Request request = Server.RequestList.ElementAt(i);

                    if (SequenceNumberIsNext(request)) {
                        Order order = new Order(request, Server.LastOrderSequenceNumber,Server.endpointURL);
                        Server.RequestList.Remove(request);
                        BroadcastOrder(order);
                        Decide(); // check again if there are more
                        return;
                    }

                }
            }
        }

        // Check if there are requests with the same endpointURL (client identifier) that can be executed
        public void Decide(string endpointURL) {
            lock (Server.RequestList) {

                for (int i = 0; i < Server.RequestList.Capacity; i++) {
                    Request request = Server.RequestList.ElementAt(i);

                    if (request.SrcEndpointURL.Equals(endpointURL) && (SequenceNumberIsNext(request))) {
                        Order order = new Order(request, Server.LastOrderSequenceNumber, Server.endpointURL);
                        Server.RequestList.Remove(request);
                        BroadcastOrder(order);
                        Decide(endpointURL); // check again if there are more
                        return;
                    }

                }
            }
        }

        public void BroadcastOrder(Order order) {
            RemoteAsyncDelegate remoteDel;

            for (int i = 0; i < Server.knownServerRemotes.Capacity; i++) {
                remoteDel = new RemoteAsyncDelegate(Server.knownServerRemotes.ElementAt(i).OnReceiveMessage);
                remoteDel.BeginInvoke(order, null, null);
            }
            ++Server.LastOrderSequenceNumber;
        }

        public override Message ProcessRequest(Request request) {
            Server.SaveRequest(request);
            Decide(request.SrcEndpointURL);

            return null;
        }

        

        public override Message ProcessOrder(Order order) {
            return null; 
        }
    }
}