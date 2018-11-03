using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace {
    public class ServerBehaviour : ITupleOperations{
        private Server server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            this.server = server;
        }

        public virtual Message OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Order))){
                // execute the order (tuple space operation)
            }
            else if ( message.GetType().Equals(typeof(Request))) {

                server.mostRecentClientRequestSeqNumbers.Add(message.clientRemoteURL, message.seqNum);
                server.requestQueue.Enqueue((Request)message);
            }
        }


        public void Write(Tuple tuple) {
            throw new NotImplementedException();
        }

        public Tuple Read(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }

        public Tuple Take(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }
    }
}
