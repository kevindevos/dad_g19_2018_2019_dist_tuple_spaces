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

        public ServerSMR(string host, int port) : base(host, port) 
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        public ServerSMR() : this(defaultServerHost, defaultServerPort) { }

        public ServerSMR(int serverPort) : this(defaultServerHost, serverPort) { }

        public void UpgradeToMaster()
        {
            Behaviour = new MasterServerSMRBehaviour(this);
            while(((MasterServerSMRBehaviour)Behaviour).Decide()); 
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        public override void OnReceiveMessage(Message message) {
            Log("OnReceiveMessage called, message: " + message);

            if (message.GetType() == typeof(Request)) {
                Behaviour.ProcessRequest((Request)message);
                return;
            }

            if (message.GetType() == typeof(Order)) {
                Behaviour.ProcessOrder((Order)message);
                return;
            }
            
            throw new NotImplementedException();
        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
        
    }
}