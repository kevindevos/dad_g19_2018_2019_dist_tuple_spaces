using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.XL.Behaviour;
using System.Collections.Generic;

namespace ServerNamespace.XL

{
    public class ServerXL : Server
    {
        // new hides the Behaviour of the base class Server, basically replacing the base type of Behaviour to ServerXLBehaviour here
        new ServerXLBehaviour Behaviour;

        // Workers
        public List<Worker> Workers = new List<Worker>();
        public List<TupleSpace> Replicas = new List<TupleSpace>();


        public ServerXL(string host, int port) : base(host,port)
        {
            Behaviour = new ServerXLBehaviour(this);
        }

        public ServerXL() : this(DefaultServerHost, DefaultServerPort) { }

        private ServerXL(int serverPort) : this(DefaultServerHost, serverPort) { }

        public override Message OnReceiveMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override Message OnSendMessage(Message message) {
            throw new System.NotImplementedException();
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