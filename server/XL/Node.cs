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

            // TODO assign replicas to the workers
        }

        public Node() : this(DefaultServerHost, DefaultServerPort) { }

        private Node(int serverPort) : this(DefaultServerHost, serverPort) { }

        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType() == typeof(Request)) {
                return PerformRequest((Request)message);
            }

            throw new NotImplementedException();
        }

        public Message PerformRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            switch (request.RequestType) {
                case RequestType.READ:
                    var resultTuples = Read(tupleSchema);
                    Response response = new Response(request, resultTuples, EndpointURL);
                    return response;

                case RequestType.WRITE:
                    Write(request.Tuple);

                    // Send back an Ack of the write
                    Ack ack = new Ack(EndpointURL, request);
                    SendMessageToRemoteURL(request.SrcRemoteURL, request);
                    return ack;

                case RequestType.TAKE:
                    // TODO attempt lock, if fail return reject, if not return list of possible tuples to choose from

                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Take(tupleSchema);
                    return new Response(request, resultTuples, EndpointURL);

                case RequestType.REMOVE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    Remove(tupleSchema);
                    
                    // Send back an Ack of the remove
                    ack = new Ack(EndpointURL, request);
                    SendMessageToRemoteURL(request.SrcRemoteURL, request);
                    return ack;


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

       

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }

        public override void Write(Tuple tuple) {
            foreach(Worker worker in Workers) {
                worker.Write(tuple);
            }
        }

        public override List<Tuple> Read(TupleSchema tupleSchema) {
            List<Tuple> tuples = new List<Tuple>();
            foreach (Worker worker in Workers) {
                tuples.AddRange(worker.Read(tupleSchema));
            }
            return tuples;
        }

        public override List<Tuple> Take(TupleSchema tupleSchema) {
            List<Tuple> tuples = new List<Tuple>();
            foreach (Worker worker in Workers) {
                tuples.AddRange(worker.Take(tupleSchema));
            }
            return tuples;
        }

        private void Remove(TupleSchema tupleSchema) {
            foreach (Worker worker in Workers) {
                worker.Remove(tupleSchema);
            }
        }
    }
}