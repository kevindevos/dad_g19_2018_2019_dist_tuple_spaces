using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace {
    public class ServerBehaviour {
        private Server server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            this.server = server;
        }

        public virtual Message OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Order))){
                // find the Request object for the order pair client seqnumber and id
                Request request = server.getRequestBySeqNumberAndClientUrl(message.seqNum, message.clientRemoteURL);
                // remove from the requestList
                server.removeRequestFromList(request);

                switch (request.requestType) {
                    case RequestType.READ:
                        TupleSchema tupleSchema = new TupleSchema(request.tuple);
                        List<Tuple> resultTuples = Read(tupleSchema);
                        return new Response(message.seqNum, message.clientRemoteURL, resultTuples);

                    case RequestType.WRITE:
                        Write(message.tuple);
                        return null;

                    case RequestType.TAKE:
                        tupleSchema = new TupleSchema(request.tuple);
                        resultTuples = Take(tupleSchema);
                        return new Response(message.seqNum, message.clientRemoteURL, resultTuples); 
                }
            }
            else if ( message.GetType().Equals(typeof(Request))) {
                server.mostRecentClientRequestSeqNumbers.Add(message.clientRemoteURL, message.seqNum);
                server.requestList.Add((Request)message);

                // Problem, when client does read or take, it is blocking, it expects a return message, but the server needs to wait for the order of the master, ?!?!?

            }
            return null;
        }


        public void Write(Tuple tuple) {
            throw new NotImplementedException();
        }

        public List<Tuple> Read(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }

        public List<Tuple> Take(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }
    }
}
