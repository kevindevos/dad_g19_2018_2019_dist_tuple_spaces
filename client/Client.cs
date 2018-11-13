using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public delegate Message SendMessageDelegate(Message message);

    public abstract class Client : RemotingEndpoint {
        protected int ClientRequestSeqNumber;
        // <Request.sequenceNumber, Response>
        protected readonly Dictionary<int, Response> ReceivedResponses;
        
        protected const string ClientObjName = "Client";

        // <Request.sequenceNumber, Semaphore>
        protected readonly Dictionary<int, SemaphoreSlim> RequestSemaphore;
        
        public Client() : this(DefaultClientHost, DefaultClientPort) { }
        
        public Client(string host, int port) : this(BuildRemoteUrl(host,port,ClientObjName)) {
        }

        protected Client(string remoteUrl) : base(remoteUrl){
            ReceivedResponses = new Dictionary<int, Response>();
            ClientRequestSeqNumber = 0;
            RequestSemaphore = new Dictionary<int, SemaphoreSlim>();
        }

        public abstract void Write(Tuple tuple);
        public abstract Tuple Read(Tuple tuple);
        public abstract Tuple Take(Tuple tuple);


        public virtual void Log(string text) {
            Console.WriteLine("[CLIENT:" + EndpointURL + "] : " + text);
        }
    }
}