using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace {
    class MasterServerBehaviour : ServerBehaviour {
        private Server server;

        public MasterServerBehaviour(Server server) : base(server) {
            this.server = server;
        }

       
        public override void OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Request))) {
                server.mostRecentClientRequestSeqNumbers.Add(message.clientId, message.seqNum);
                server.requestQueue.Enqueue((Request)message);

                broadcastRequestAsOrder((Request)message);

                // TODO , perform the tuple space action here , after broadcasting the order
            }

            base.OnReceiveMessage(message);
        }

        private void broadcastRequestAsOrder(Request message) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
    