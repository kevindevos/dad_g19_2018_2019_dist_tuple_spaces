using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes.message;
using CommonTypes.tuple;
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

        public ServerXL(string remoteUrl) : base(remoteUrl){
            LockedTuples = new ConcurrentDictionary<int, List<Tuple>>();
        }

        
        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType().IsSubclassOf(typeof(Request))) {
                return ProcessRequest((Request) message);
            }

            if (message.GetType() == typeof(LockRelease)){
                ProcessLockRelease((LockRelease) message);
                return null;
            }

            if (message.GetType() == typeof(TakeRemove)){
                return ProcessTakeRemove(message);
            }

            throw new NotImplementedException();
        }

        /**
         * Move from LockedTuples to the tuplespace the "locked" tuples by the take request
         */
        private void ProcessLockRelease(LockRelease lockRelease){
            ReleaseLockedTuples(lockRelease.TakeRequestSeqNum);
        }

        /**
         * Remove the selected tuple and add the remaining locked tuples back to the tuplespace
         */
        private Message ProcessTakeRemove(Message message){
            TakeRemove takeRemove = (TakeRemove) message;
            LockedTuples.TryRemove(takeRemove.TakeRequestSeqNumber, out List<Tuple> lockedTuples);

            // remove the selected tuple specified in the request
            if (lockedTuples != null && takeRemove.SelectedTuple != null){
                lockedTuples.Remove(takeRemove.SelectedTuple);

                // add back the remaining tuples to the tuple space
                foreach (Tuple tuple in lockedTuples){
                    TupleSpace.Write(tuple);
                }
            }

            // Send back an Ack of the remove
            Ack ack = new Ack(EndpointURL, takeRemove);
            SendMessageToRemoteURL(takeRemove.SrcRemoteURL, ack);
            return ack;
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

        public Message ProcessRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            if (request.GetType() == typeof(WriteRequest)){
                TupleSpace.Write(request.Tuple);
                
                // notify a blocked thread that tried to read/take this tuple before
                Request readOrTakeRequest = GetAndRemoveSinglePendingRequest(request.Tuple);
                if (readOrTakeRequest != null){
                    PulseMessage(readOrTakeRequest);
                }

                // Send back an Ack of the write
                return new Ack(EndpointURL, request);
            }

            if (request.GetType() == typeof(ReadRequest)){
                List<Tuple> tuples = TupleSpace.Read(tupleSchema);

                while (!tuples.Any()){
                    lock (PendingRequestList){
                        PendingRequestList.Add(request);
                    }
                    WaitMessage(request, View.Nodes);
                    tuples = TupleSpace.Read(tupleSchema);
                }

                Response response = new Response(request, tuples, EndpointURL);
                return response;
            }

            if (request.GetType() == typeof(TakeRequest)){
                tupleSchema = new TupleSchema(request.Tuple);
                List<Tuple> resultTuples = TupleSpace.Take(tupleSchema);
                Response response = null;
                
                while (!resultTuples.Any()){
                    lock (PendingRequestList){
                        PendingRequestList.Add(request);
                    }
                    WaitMessage(request, View.Nodes);
                    resultTuples = TupleSpace.Take(tupleSchema);
                }
                    
                // "lock" the tuples taken from the tuple space
                LockedTuples.TryAdd(request.SeqNum, resultTuples);
                response = new Response(request, resultTuples, EndpointURL);
                SendMessageToRemoteURL(request.SrcRemoteURL, response);

                return response;
            }

            throw new NotImplementedException();
        }

        /**
         * Searches the pending list of read and take requests for any request that tried to read/take this tuple
         */
        private Request GetAndRemoveSinglePendingRequest(Tuple tuple){
            lock (PendingRequestList){
                foreach (Request req in PendingRequestList){
                    if (req.GetType() == typeof(ReadRequest) || req.GetType() == typeof(TakeRequest) && req.Tuple.Equals(tuple)){
                        return req;
                    }
                }
            }

            return null;
        }
    }
}