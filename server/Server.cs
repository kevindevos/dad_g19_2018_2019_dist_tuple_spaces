using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace {
    public abstract class Server : RemotingEndpoint {
        public delegate Message SendMessageDelegate(Message message);

        protected const string ServerObjName = "Server";
        private readonly int _minDelay;
        private readonly int _maxDelay;
        private readonly Random _rnd = new Random();

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> RequestList { get; }
        
        // a list of pending requests for some reason
        public List<Request> PendingRequestList { get; }
        
        // Tuple space
        protected TupleSpace TupleSpace { get; } = new TupleSpace();

        protected Server() : this(DefaultServerHost, DefaultServerPort){}
        private Server(int serverPort) : this(DefaultServerHost, serverPort){}
        protected Server(string host, int port) : this(BuildRemoteUrl(host, port, ServerObjName)) {}
        public Server(string remoteUrl, IEnumerable<string> knownServerUrls = null, int minDelay = 0,
            int maxDelay = 0) : base(remoteUrl, knownServerUrls){
            RequestList = new List<Request>();
            PendingRequestList = new List<Request>();
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            
            HeartbeatCheckerThread = new Thread(CheckBeats);
            HeartbeatCheckerThread.Start();
            
            BeatThread = new Thread(DoBeat);
            BeatThread.Start();
            
        }

        public void SaveRequest(Request request) {
            lock (RequestList) {
                RequestList.Add(request);
            }
        }

        public void DeleteRequest(Request request) {
            lock (RequestList) {
                RequestList.Remove(request);
            }
        }

        protected void FreezeCheck()
        {
            FreezeLock.Wait();
            FreezeLock.Release();
        }

        protected void DelayCheck()
        {
            var delay = _rnd.Next(_minDelay, _maxDelay);
            if(delay > 0) Thread.Sleep(delay);
        }

        public virtual void Log(string text) {
            Console.WriteLine("[SERVER:"+EndpointURL +"]   " + text);
        }

        public string Status()
        {
            string response = "";
            HashSet<string> nodes;
            long version;
            Dictionary<int, List<Tuple>> tupleSpace = TupleSpace.GetCopy();
            
            lock (View)
            {
                nodes = new HashSet<string>(View.Nodes);
                version = View.Version;
            }

            response += $"[VIEW version={version}]\n";
            foreach (var node in nodes)
            {
                response += $"  {node}\n";
            }
            
            response += $"[TUPLE SPACE]\n";
            foreach (var size in tupleSpace.Keys.ToArray())
            {
                response += $"  [size={size}]\n";

                foreach (var tuple in tupleSpace[size])
                {
                    response += $"    {tuple.ToString()}\n";
                }
            }
            
            response += "===============\n";
            return response;
        }
    }

}