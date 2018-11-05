using CommonTypes;
using ServerNamespace.Behaviour.SMR;

namespace ServerNamespace.Server
{
    public class ServerSmr : Server
    {
        public ServerSmr() : base()
        {
            Behaviour = new NormalServerSmrBehaviour(this);
        }

        
        public void UpgradeToMaster()
        {
            Behaviour = new MasterServerSmrBehaviour(this);
        }

        public void DowngradeToNormal()
        {
            Behaviour = new NormalServerSmrBehaviour(this);
        }
        
        
    }
}