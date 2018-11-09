using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.XL.Behaviour;
using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace.XL

{
    // ServerXL
    public class Node : Server
    {
        // new hides the Behaviour of the base class Server, basically replacing the base type of Behaviour to ServerXLBehaviour here
        new ServerXLBehaviour Behaviour;

        // Workers that carry out operations on a replica
        public List<Worker> Workers;

        // set of tuple space replicas
        public List<TupleSpace> Replicas;

        // The view, the agreed set of replicas
        public List<TupleSpace> View; // must be the same across all nodes 

        public Node(string host, int port) : base(host,port)
        {
            Behaviour = new ServerXLBehaviour(this);
            Workers = new List<Worker>();
            View = new List<TupleSpace>();
        }

        public Node() : this(DefaultServerHost, DefaultServerPort) { }

        private Node(int serverPort) : this(DefaultServerHost, serverPort) { }

        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType() == typeof(Request)) {

                // TODO
                return null;
            }

            return null;
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