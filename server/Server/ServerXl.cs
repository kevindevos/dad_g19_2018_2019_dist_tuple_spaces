using ServerNamespace.Behaviour.XL;

namespace ServerNamespace.Server
{
    public class ServerXl : Server
    {
        public ServerXl()
        {
            Behaviour = new NormalServerXlBehaviour(this);
        }

    }
}