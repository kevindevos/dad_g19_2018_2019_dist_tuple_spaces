using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.XL.Behaviour {
    public class ServerXLBehaviour : ServerBehaviour {
        public ServerXLBehaviour(Server server) : base(server) {
        }

        public override Response PerformRequest(Request request) {
            throw new System.NotImplementedException();
        }

        public override Message ProcessMessage(Message message) {
            throw new System.NotImplementedException();
        }
    }
}