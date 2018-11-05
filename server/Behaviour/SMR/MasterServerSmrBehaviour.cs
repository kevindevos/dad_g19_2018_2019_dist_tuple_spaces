using System;
using System.Linq;
using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class MasterServerSMRBehaviour : ServerBehaviour {
        
        public MasterServerSmrBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message ProcessMessage(Message message) {
            if (message.GetType().Equals(typeof(Request))) {
                Request request = (Request)message;
                Server.SaveRequest(request);
                Decide();
            }
            return null;
        }

        private void Decide() {
            lock (Server.RequestList) {
                for (int i = 0; i < Server.RequestList.Capacity; i++) {
                    Request request = Server.RequestList.ElementAt(i);

                    // is the request's sequence number the one after the last executed request of the same client? (client order)
                    if (ClientRequestSequenceNumberIsValid(request)) {
                        Order order = new Order(request, Server.LastOrderSequenceNumber);
                        Server.RequestList.Remove(request);
                        BroadcastOrder(order);
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

       
    }
}