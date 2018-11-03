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

        public override Message OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Request))) {
                server.mostRecentClientRequestSeqNumbers.Add(message.clientRemoteURL, message.seqNum);
                server.requestList.Add((Request)message);

                // TODO Problem, when client does read or take, it is blocking, it expects a return message, but request is delayed for the master decision
                // return ???
            }
            return null;

        }

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool isClientRequestSeqNumValid(Request request) {
            // TODO
            throw new NotImplementedException();
        }

        // send an Order object to all other servers to perform the request
        public void broadcastRequestAsOrder(Request request) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
    