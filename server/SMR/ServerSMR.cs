using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;
using ServerNamespace.SMR.Behaviour;
using Tuple = CommonTypes.tuple.Tuple;
using System.Linq;

namespace ServerNamespace
{
    public class ServerSMR : Server
    {
        public int LastOrderSequenceNumber { get; set; }

        public List<RemotingEndpoint> OtherServers { get; private set; }

        // A dictionary containing the most recent executed requests of the most recent remoteurls of each client.  <clientRemoteURL, Reques>
        public ConcurrentDictionary<string, Request> LastExecutedRequests { get; }

        // A dictionary containing the most recent executed orders of the most recent requests of each client.  <clientRemoteURL, Order>
        public ConcurrentDictionary<string, Order> LastExecutedOrders { get; private set; }

        public List<Order> SavedOrders { get; }

        protected List<Ack> ReceivedAcks { get; private set; }

        public string MasterEndpointURL { get; set; }

        // Tuple space
        private TupleSpace TupleSpace { get; }

        public ServerSMRBehaviour Behaviour;

        
        public ServerSMR() : this(DefaultServerPort) { }
        
        public ServerSMR(int serverPort) : this(DefaultServerHost, serverPort) { }

        private ServerSMR(string host, int port) : this(BuildRemoteUrl(host, port, ServerObjName)) 
        {
            
        }

        public ServerSMR(string remoteUrl) : base(remoteUrl){
            Behaviour = new NormalServerSMRBehaviour(this);
            LastExecutedRequests = new ConcurrentDictionary<string, Request>();
            LastExecutedOrders = new ConcurrentDictionary<string, Order>();
            SavedOrders = new List<Order>();
            LastOrderSequenceNumber = 0;
            TupleSpace = new TupleSpace();
            ReceivedAcks = new List<Ack>();
            
            
            RecursiveJoinView(View);
            getMaster();
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

        // ask the view for a master
        public void getMaster()
        {
            String masterUrl = null;
            
            foreach (var server in View)
            {
                var message = new Elect(EndpointURL, ElectType.GET_MASTER);
                var result = (Elect)SendMessageToRemote(server, message);
                masterUrl = result.NewMasterURL;
                if (masterUrl != null)
                    break;
            }

            if (masterUrl != null)
            {
                MasterEndpointURL = masterUrl;
                Log("Master is at: " + masterUrl);
            }
            else 
                UpgradeToMaster();
        }


        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);
            
            // if an Elect message, define new master as the one included in the Elect message
            if (message.GetType() == typeof(Elect)) {
                return ProcessElect((Elect)message);
            }

            if (message.GetType().IsSubclassOf(typeof(Request))) {
                return Behaviour.ProcessRequest((Request)message);
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

        public void Write(Tuple tuple) {
            TupleSpace.Write(tuple);
            Log("Wrote : " + tuple);
        }

        public void Write(List<Tuple> tuples) {
            foreach (Tuple tuple in tuples) {
                Write(tuple);
            }
        }

        public List<Tuple> Read(TupleSchema tupleSchema) {
            var listTuple = TupleSpace.Read(tupleSchema);
            if (listTuple.Count > 0) {
                Log("Read (first tuple): " + listTuple.First());
            }
            return listTuple;
        }

        public List<Tuple> Take(TupleSchema tupleSchema) {
            List<Tuple> tuples = TupleSpace.Take(tupleSchema);
            if (tuples.Count > 0) {
                List<Tuple> tuplesWriteBack = new List<Tuple>(tuples);
                tuplesWriteBack.Remove(tuplesWriteBack.First());
                Write(tuplesWriteBack);
                Log("Took (first tuple): " + tuples.First());
            }

            return tuples;
        }
        public Response ProcessRequest(Request request) {
            var tupleSchema = new TupleSchema(request.Tuple);

            if (request.GetType() == typeof(ReadRequest)){
                var resultTuples = Read(tupleSchema);
                return new Response(request, resultTuples, EndpointURL);
            }
            
            if (request.GetType() == typeof(WriteRequest)){
                Write(request.Tuple);
                return null;
            }
            
            if (request.GetType() == typeof(TakeRequest)){
                tupleSchema = new TupleSchema(request.Tuple);
                List<Tuple> resultTuples = Take(tupleSchema);
                return new Response(request, resultTuples, EndpointURL);
            }
            
            throw new ArgumentOutOfRangeException();
        }

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
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

    }
}