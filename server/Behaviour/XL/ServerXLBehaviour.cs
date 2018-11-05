using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.XL
{
    public class ServerXLBehaviour : ServerBehaviour
    {
        public ServerXLBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message ProcessMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}