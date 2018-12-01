using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace.XL
{
    // ServerXL
    public class ServerXL : Server{
        
        public readonly TupleSpace TupleSpace = new TupleSpace();

        public ServerXL() : this(DefaultServerHost, DefaultServerPort) { }

        private ServerXL(int serverPort) : this(DefaultServerHost, serverPort) { }

        public ServerXL(string host, int port) : this(BuildRemoteUrl(host,port, ServerObjName)) { }

        public ServerXL(string remoteUrl) : base(remoteUrl) { }

        
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
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Take(tupleSchema);
                    return new Response(request, resultTuples, EndpointURL);

                case RequestType.REMOVE:
                    Remove(request.Tuple);
                    
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
            TupleSpace.Write(tuple);
        }

        public override List<Tuple> Read(TupleSchema tupleSchema){
            return TupleSpace.Read(tupleSchema);
        }

        public override List<Tuple> Take(TupleSchema tupleSchema){
            return TupleSpace.Take(tupleSchema);
        }

        private void Remove(Tuple tuple){
            TupleSpace.Remove(tuple);
        }
    }
}