using CommonTypes;
using CommonTypes.message;
using ServerNamespace.Behaviour.SMR;

namespace ServerNamespace
{
    public class ServerSMR : Server
    {
        public ServerSMR() 
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        
        public void UpgradeToMaster()
        {
            Behaviour = new MasterServerSMRBehaviour(this);
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSMRBehaviour(this);
        }

        public override void OnReceiveMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}