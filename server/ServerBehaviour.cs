<<<<<<< HEAD
 using CommonTypes;
using CommonTypes.message;
using System;
using System.Collections.Generic;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace {
    public class ServerBehaviour {
        protected Server server; 

        public ServerBehaviour(Server server) {
            this.server = server;
        }

        public virtual Message OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Order))){
                Order order = (Order)message;
                server.DeleteRequest(order.Request);
                server.LastOrderSequenceNumber = order.OrderSeqNumber;

                return PerformRequest(order.Request);
            }
            else if ( message.GetType().Equals(typeof(Request))) {
                Request request = (Request)message;
                server.SaveRequest(request);

                // TODO Problem, when client does read or take, it is blocking, it expects a return message, but the server needs to wait for the order of the master, ?!?!?

            }
            return null;
        }

       public Response PerformRequest(Request request) {
            switch (request.RequestType) {
                case RequestType.READ:
                    TupleSchema tupleSchema = new TupleSchema(request.Tuple);
                    List<Tuple> resultTuples = Read(tupleSchema);
                    return new Response(request, resultTuples);

                case RequestType.WRITE:
                    Write(request.Tuple);
                    return null;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Take(tupleSchema);
                    return new Response(request, resultTuples);
            }
            return null;
        }

        private void Write(Tuple tuple) {
            throw new NotImplementedException();
        }

        private List<Tuple> Read(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }

        private List<Tuple> Take(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }
    }
}
=======
﻿
>>>>>>> 8f6f601164ee3244ab3007ec0a0856b7274d638e
