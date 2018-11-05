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
                // find the Request object for the order pair client seqNumber and id
                var request = Server.GetRequestBySeqNumberAndClientUrl(message.seqNum, message.clientRemoteURL);
                return PerformRequest(request);
            }
            else if ( message.GetType() == typeof(Request)) {
                Server.MostRecentClientRequestSeqNumbers.Add(message.clientRemoteURL, message.seqNum);
                Server.RequestList.Add((Request)message);

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