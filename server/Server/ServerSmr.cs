using CommonTypes;
using CommonTypes.message;
using ServerNamespace.Behaviour.SMR;
using System.Collections.Concurrent;

namespace ServerNamespace
{
    public class ServerSMR : Server
    {
        public ServerSMR(string host, int port) : base(host, port) 
        {
            Behaviour = new NormalServerSMRBehaviour(this);
            
        }

        public ServerSMR() : base(defaultServerHost, 8086) { }

        private ServerSMR(int serverPort) : base(defaultServerHost, serverPort) { }

        public void UpgradeToMaster()
        {
            Behaviour = new MasterServerSMRBehaviour(this);
            ((MasterServerSMRBehaviour)Behaviour).Decide(); 
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }
        
        public override void OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Request))) {
                Behaviour.ProcessRequest((Request)message);
            }

            if (message.GetType().Equals(typeof(Order))) {
                Behaviour.ProcessOrder((Order)message);
            }


        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}