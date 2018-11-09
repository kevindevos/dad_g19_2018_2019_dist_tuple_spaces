using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using CommonTypes;
using CommonTypes.message;
using ServerNamespace.Behaviour.SMR;

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

        // new hides the Behaviour of the base class Server, basically replacing the base type of Behaviour to ServerSMRBehaviour here
        new ServerSMRBehaviour Behaviour;

        public ServerSMR(int serverPort) : this(DefaultServerHost, serverPort) { }
        
        public ServerSMR(string host, int port) : base(host, port) 
        {
            Behaviour = new NormalServerSMRBehaviour(this);
            LastExecutedRequests = new ConcurrentDictionary<string, Request>();
            LastExecutedOrders = new ConcurrentDictionary<string, Order>();
            SavedOrders = new List<Order>();
            LastOrderSequenceNumber = 0;
        }

        public void UpgradeToMaster()
        {
            Behaviour = new MasterServerSMRBehaviour(this);
            while(((MasterServerSMRBehaviour)Behaviour).Decide()); 
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        public override Message OnReceiveMessage(Message message) {
            Log("Received message: " + message);

            if (message.GetType() == typeof(Request)) {
                return Behaviour.ProcessRequest((Request)message);
            }

            if (message.GetType() == typeof(Order)) {
                return Behaviour.ProcessOrder((Order)message);
            }
            
            throw new NotImplementedException();
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