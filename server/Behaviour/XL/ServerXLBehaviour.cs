using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.XL
{
    public class NormalServerXlBehaviour : ServerBehaviour
    {
        public NormalServerXlBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message ProcessMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}