using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.SMR.Behaviour;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace
{
    public class ServerSMR : Server
    {
        public int LastOrderSequenceNumber { get; set; }

        // A dictionary containing the most recent executed requests of the most recent remoteurls of each client.  <clientRemoteURL, Reques>
        public ConcurrentDictionary<string, Request> LastExecutedRequests { get; }

        // A dictionary containing the most recent executed orders of the most recent requests of each client.  <clientRemoteURL, Order>
        public ConcurrentDictionary<string, Order> LastExecutedOrders { get; private set; }
        public List<Order> SavedOrders { get; }
        protected List<Ack> ReceivedAcks { get; private set; }
        
        public string MasterEndpointURL { get; set; }

        public ServerSMRBehaviour Behaviour;

        public ServerSMR() : this(DefaultServerPort) { }
        public ServerSMR(int serverPort) : this(DefaultServerHost, serverPort) { }
        private ServerSMR(string host, int port) : this(BuildRemoteUrl(host, port, ServerObjName)) {}

        public ServerSMR(string remoteUrl, IEnumerable<string> knownServerUrls = null, int minDelay = 0,
            int maxDelay = 0) : base(remoteUrl, knownServerUrls, minDelay, maxDelay)
        {
            Behaviour = new NormalServerSMRBehaviour(this);
            LastExecutedRequests = new ConcurrentDictionary<string, Request>();
            LastExecutedOrders = new ConcurrentDictionary<string, Order>();
            SavedOrders = new List<Order>();
            LastOrderSequenceNumber = 0;
            ReceivedAcks = new List<Ack>();

            CheckMaster();
        }

        public void UpgradeToMaster()
        {
            Log("Upgrade to master.");
            MasterEndpointURL = EndpointURL;
            Behaviour = new MasterServerSMRBehaviour(this);
            while(((MasterServerSMRBehaviour)Behaviour).Decide()); 
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        private void CheckMaster()
        {
            List<string> viewCopy;
            lock (View)
            {
                viewCopy = new List<string>(View.Nodes);
            }
            viewCopy.Add(EndpointURL);
            viewCopy = viewCopy.OrderBy(s => s).ToList();
            var masterUrl = viewCopy.First();
            
            if (masterUrl == EndpointURL && MasterEndpointURL != EndpointURL)
            {
                UpgradeToMaster();
            }
            else if(masterUrl != EndpointURL && MasterEndpointURL == EndpointURL)
            {
                Log("Master is at: " + masterUrl);
                DowngradeToNormal();
            }
            MasterEndpointURL = masterUrl;

        }

        public override Message OnReceiveMessage(Message message)
        {
            FreezeCheck();
            DelayCheck();
            
            Log("Received message from : " + message.SrcRemoteURL);
            
            
            // if an Elect message, define new master as the one included in the Elect message
            if (message.GetType() == typeof(Elect)) {
                return ProcessElect((Elect)message);
            }

            if (message.GetType().IsSubclassOf(typeof(Request))) {
                Behaviour.ProcessRequest((Request)message);
                return message; //TODO
            }

            if (message.GetType() == typeof(Order)) {
                return Behaviour.ProcessOrder((Order)message);
            }

            

            throw new NotImplementedException();
        }

        private Message ProcessElect(Elect message)
        {
            switch (message.ElectType)
            {
                case ElectType.GET_MASTER:
                    message.NewMasterURL = MasterEndpointURL;
                    return message;
                case ElectType.SET_MASTER:
                    MasterEndpointURL = message.NewMasterURL;
                    return message;
            }
            throw new NotImplementedException();
        }

        private void Write(Tuple tuple) {
            TupleSpace.Write(tuple);
            Log("Wrote : " + tuple);
        }

        private void Write(List<Tuple> tuples) {
            foreach (Tuple tuple in tuples) {
                Write(tuple);
            }
        }

        private List<Tuple> Read(TupleSchema tupleSchema) {
            var listTuple = TupleSpace.Read(tupleSchema);
            if (listTuple.Count > 0) {
                Log("Read (first tuple): " + listTuple.First());
            }
            return listTuple;
        }

        private List<Tuple> Take(TupleSchema tupleSchema) {
            List<Tuple> tuples = TupleSpace.Take(tupleSchema);
            if (tuples.Count > 0) {
                List<Tuple> tuplesWriteBack = new List<Tuple>(tuples);
                tuplesWriteBack.Remove(tuplesWriteBack.First());
                Write(tuplesWriteBack);
                Log("Took (first tuple): " + tuples.First());
            }

            return tuples;
        }
        public Message ProcessRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            if (request.GetType() == typeof(WriteRequest)){
                Write(request.Tuple);
                return new Response(request, new List<Tuple>(), EndpointURL); 
            }

            if (request.GetType() == typeof(ReadRequest)){
                var resultTuples = Read(tupleSchema);
                return new Response(request, resultTuples, EndpointURL);
            }

            if (request.GetType() == typeof(TakeRequest)){
                tupleSchema = new TupleSchema(request.Tuple);
                List<Tuple> resultTuples = Take(tupleSchema);
                return new Response(request, resultTuples, EndpointURL);
            }
            
            throw new ArgumentOutOfRangeException();
        }


        public override void NotifyViewChange()
        {
            CheckMaster();
        }

        public override void Log(string text) {
            string serverType = Behaviour.GetType() == typeof(MasterServerSMRBehaviour) ? "MASTER" : "NORMAL";
            Console.WriteLine("[SERVER:" + EndpointURL + "]["+serverType+"] : " + text);
        }

        public void UpdateLastExecutedOrder(Order order) {
            Order orderTemp;
            LastExecutedOrders.TryRemove(order.SrcRemoteURL, out orderTemp);
            LastExecutedOrders.TryAdd(order.SrcRemoteURL, order);
        }

        public void UpdateLastExecutedRequest(Request request) {
            Request requestTemp;
            LastExecutedRequests.TryRemove(request.SrcRemoteURL, out requestTemp);
            LastExecutedRequests.TryAdd(request.SrcRemoteURL, request);
        }

        private void WaitForWrite(Request readOrTakeRequest){
            throw new NotImplementedException();
        }

    }
}