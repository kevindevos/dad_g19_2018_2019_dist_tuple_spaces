using CommonTypes.message;
using ServerNamespace.Behaviour.XL;

namespace ServerNamespace.Server
{
    public class ServerXL : Server
    {
        public ServerXL()
        {
            Behaviour = new ServerXLBehaviour(this);
        }

        public override void OnReceiveMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}