using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace.XL
{
    // ServerXL
    public class ServerXL : Server{
        
        public readonly TupleSpace TupleSpace = new TupleSpace();
        
        // contains the tuples when a take is performed for a request with a specific sequence number
        // acts as a "lock" because these tuples would be removed from the tuple space and kept here temporarily
        private ConcurrentDictionary<int, List<Tuple>> LockedTuples;

        public ServerXL() : this(DefaultServerHost, DefaultServerPort) { }

        private ServerXL(int serverPort) : this(DefaultServerHost, serverPort) { }

        public ServerXL(string host, int port) : this(BuildRemoteUrl(host,port, ServerObjName)) { }

        public ServerXL(string remoteUrl) : base(remoteUrl) { }

        
        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType() == typeof(Request)) {
                return PerformRequest((Request)message);
            }

            if (message.GetType() == typeof(LockRelease)){
                LockRelease lockRelease = (LockRelease) message;
                ReleaseLockedTuples(lockRelease.TakeRequestSeqNum);
            }

            throw new NotImplementedException();
        }

        /**
         * Remove locked tuples from LockedTuples List, and add them back to the tuple space
         * acts as a release for the "lock" of a take request
         */
        private void ReleaseLockedTuples(int takeRequestSeqNum){
            // remove from LockedTuples
            bool isRemoved = LockedTuples.TryRemove(takeRequestSeqNum, out List<Tuple> tuples);
            
            // add back to TupleSpace
            if (isRemoved){
                foreach (Tuple tuple in tuples){
                    TupleSpace.Write(tuple);
                }
            }
            
        }

        public Message PerformRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            switch (request.RequestType) {
                case RequestType.READ:
                    var resultTuples = Read(tupleSchema);
                    
                    Response response = new Response(request, resultTuples, EndpointURL);
                    SendMessageToRemoteURL(request.SrcRemoteURL, response);
                    
                    return response;

                case RequestType.WRITE:
                    Write(request.Tuple);

                    // Send back an Ack of the write
                    Ack ack = new Ack(EndpointURL, request);
                    SendMessageToRemoteURL(request.SrcRemoteURL, ack);
                    return ack;

                case RequestType.TAKE:
                    tupleSchema = new TupleSchema(request.Tuple);
                    resultTuples = Take(tupleSchema);
                    
                    // "lock" the tuples taken from the tuple space
                    LockedTuples.TryAdd(request.SeqNum, resultTuples);
                    response = new Response(request, resultTuples, EndpointURL);
                    SendMessageToRemoteURL(request.SrcRemoteURL, response);

                    return response;

                case RequestType.REMOVE:
                    // remove from LockedTuples without adding back to tuple space
                    LockedTuples.TryRemove(request.SeqNum, out List<Tuple> tuples);
                    
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

    }
}