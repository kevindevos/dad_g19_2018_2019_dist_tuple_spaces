using CommonTypes;

namespace ServerNamespace.Behaviour.XL
{
    public class NormalServerXlBehaviour : ServerBehaviour
    {
        public NormalServerXlBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message OnReceiveMessage(Message message)
        {
            throw new System.NotImplementedException();
        }

        public override Message OnSendMessage(Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}