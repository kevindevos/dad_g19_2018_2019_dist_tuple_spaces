using CommonTypes;
using CommonTypes.message;

namespace ServerNamespace.Behaviour.SMR
{
    public class NormalServerSmrBehaviour : ServerBehaviour
    {
        public NormalServerSmrBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message OnReceiveMessage(Message message) {
            if (message.GetType() == typeof(Order)){
                var request = (Request)message;
                Server.MostRecentClientRequestSeqNumbers.Add(request.ClientRemoteUrl, request.SeqNum);
                Server.RequestList.Add(request);
                Server.Decide(); // concurrency, 
            }
            else if ( message.GetType() == typeof(Request)) {
                Request request = (Request)message;
                Server.MostRecentClientRequestSeqNumbers.Add(request.ClientRemoteUrl, request.SeqNum);
                Server.RequestList.Add(request);

                // TODO Problem, when client does read or take, it is blocking, it expects a return message, but the server needs to wait for the order of the master, ?!?!?

            }
            return null;
        }

        public override Message OnSendMessage(Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}