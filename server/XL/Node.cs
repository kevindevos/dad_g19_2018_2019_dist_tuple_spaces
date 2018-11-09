using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace.XL
{
    // ServerXL
    public class Node : Server
    {
        // Workers that carry out operations on a replica
        public List<Worker> Workers;

        // set of tuple space replicas
        public List<TupleSpace> Replicas;

        // The view, the agreed set of replicas
        public List<TupleSpace> View; // must be the same across all nodes 

        public Node(string host, int port) : base(host,port)
        {
            Workers = new List<Worker>();
            View = new List<TupleSpace>();
        }

        public Node() : this(DefaultServerHost, DefaultServerPort) { }

        private Node(int serverPort) : this(DefaultServerHost, serverPort) { }

        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType() == typeof(Request)) {
                return ProcessRequest((Request)message);
            }

            throw new NotImplementedException();
        }

        private Message ProcessRequest(Request message) {
            throw new NotImplementedException();
        }

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }

        public override void Write(Tuple tuple) {
            throw new System.NotImplementedException();
        }

        public override List<Tuple> Read(TupleSchema tupleSchema) {
            throw new System.NotImplementedException();
        }

        public override List<Tuple> Take(TupleSchema tupleSchema) {
            throw new System.NotImplementedException();
        }
    }
}