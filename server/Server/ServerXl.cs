using CommonTypes.message;
using ServerNamespace.Behaviour.XL;

namespace ServerNamespace
{
    public class ServerXL : Server
    {
        public ServerXL(string host, int port) : base(host,port)
        {
            Behaviour = new ServerXLBehaviour(this);
        }

        public ServerXL() : base(defaultServerHost, 8086) { }

        private ServerXL(int serverPort) : base(defaultServerHost, serverPort) { }

        public override void OnReceiveMessage(Message message) {
            throw new System.NotImplementedException();
        }

        public override void OnSendMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}