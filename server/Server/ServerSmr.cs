using System;
using System.Diagnostics;
using CommonTypes;
using CommonTypes.message;
using ServerNamespace.Behaviour.SMR;

namespace ServerNamespace
{
    public class ServerSMR : Server
    {
        // new hides the Behaviour of the base class Server, basically replacing the base type of Behaviour to ServerSMRBehaviour here
        new ServerSMRBehaviour Behaviour;

        public ServerSMR(int serverPort) : this(DefaultServerHost, serverPort) { }
        
        public ServerSMR(string host, int port) : base(host, port) 
        {
            Behaviour = new NormalServerSMRBehaviour(this);
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

            if(message.GetType() == typeof(Ack)) {
                ReceivedAcks.Add((Ack)message);
            }

            if (message.GetType() == typeof(Request)) {
                return Behaviour.ProcessRequest((Request)message);
            }

            if (message.GetType() == typeof(Order)) {
                return Behaviour.ProcessOrder((Order)message);
            }
            
            throw new NotImplementedException();
        }

        public override Message OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override void Log(string text) {
            string serverType = Behaviour.GetType() == typeof(MasterServerSMRBehaviour) ? "MASTER" : "NORMAL";
            Console.WriteLine("[SERVER:" + EndpointURL + "]["+serverType+"] : " + text);
        }

    }
}