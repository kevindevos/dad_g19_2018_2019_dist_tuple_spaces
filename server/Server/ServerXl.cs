using CommonTypes.message;
using ServerNamespace.Behaviour.XL;

namespace ServerNamespace
{
    public class ServerXL : Server
    {
        // new hides the Behaviour of the base class Server, basically replacing the base type of Behaviour to ServerXLBehaviour here
        new ServerXLBehaviour Behaviour;

        public ServerXL(string host, int port) : base(host,port)
        {
            Behaviour = new ServerXLBehaviour(this);
        }

        public ServerXL() : this(defaultServerHost, defaultServerPort) { }

        private ServerXL(int serverPort) : this(defaultServerHost, serverPort) { }

        public override void OnReceiveMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}