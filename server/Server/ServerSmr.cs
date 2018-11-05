using CommonTypes;
using ServerNamespace.Behaviour.SMR;

namespace ServerNamespace.Server
{
    public class ServerSMR : Server
    {
        public ServerSMR() : base()
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
        
        
    }
}