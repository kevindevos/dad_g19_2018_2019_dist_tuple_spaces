using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace {
    class MasterServerBehaviour : ServerBehaviour {
        //TODO master seq number to normal servers in Orders
        public MasterServerBehaviour(Server server) : base(server) { }

        public override Message OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Request))) {
                Request request = (Request)message;
                server.SaveRequest(request);
                decide();
            }
            return null;

        }

        private void decide() {
            lock (server.requestList) {
                for (int i = 0; i < server.RequestList.Capacity; i++) {
                    Request request = server.RequestList.ElementAt(i);

                    // is the request's sequence number the one after the last executed request of the same client? (client order)
                    if (ClientRequestSequenceNumberIsValid(request)) {
                        Order order = new Order(request, server.LastOrderSequenceNumber);
                        server.RequestList.Remove(request);
                        broadcastOrder(order);
                    }

                }
            }
        }

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool ClientRequestSequenceNumberIsValid(Request request) {
            Request lastExecutedRequest;
            return server.lastExecutedClientRequests.TryGetValue(request.ClientRemoteURL, out lastExecutedRequest) && lastExecutedRequest.SeqNum == request.SeqNum - 1;
        }

        // send an Order object to all other servers to perform the request
        public void broadcastOrder(Order order) {
            RemoteAsyncDelegate remoteDel;

            for (int i = 0; i < server.knownServerRemotes.Capacity; i++) {
                remoteDel = new RemoteAsyncDelegate(server.knownServerRemotes.ElementAt(i).OnReceiveMessage);
                remoteDel.BeginInvoke(order, null, null);
            }
            ++server.LastOrderSequenceNumber;
        }
    }
}
    