using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace {
    public class ServerBehaviour {
        private Server server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            this.server = server;
        }

        public virtual List<object> Read(List<object> members) {
            // find all tuples that contain all objects in members 
            // return 


            return null;
        }

        // TODO other tuple operations

        public virtual void OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Order))){
                // execute the order (tuple space operation)
            }
            else if ( message.GetType().Equals(typeof(Request))) { 
                server.mostRecentClientRequestSeqNumbers.Add(message.clientId, message.seqNum);
                server.requestQueue.Enqueue((Request)message);
            }
        }

    }
}
