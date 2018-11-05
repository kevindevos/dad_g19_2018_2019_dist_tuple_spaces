using System;
using CommonTypes;

namespace ServerNamespace.Behaviour.SMR
{
    public class MasterServerSmrBehaviour : ServerBehaviour {
        
        public MasterServerSmrBehaviour(Server.Server server) : base(server)
        {
        }

        public override Message OnReceiveMessage(Message message)
        {
            if (message.GetType() != typeof(Request)) return null;
            
            Server.MostRecentClientRequestSeqNumbers.Add(message.clientRemoteURL, message.seqNum);
            Server.RequestList.Add((Request)message);

            // TODO Problem, when client does read or take, it is blocking, it expects a return message, but request is delayed for the master decision
            // return ???
            return null;
        }

        public override Message OnSendMessage(Message message)
        {
            throw new NotImplementedException();
        }
        

        // check if the sequence number of this request is just 1 higher than the previous ( else there is a missing request )
        public bool IsClientRequestSeqNumValid(Request request) {
            // TODO
            throw new NotImplementedException();
        }

        // send an Order object to all other servers to perform the request
        public void BroadcastRequestAsOrder(Request request) {
            // TODO
            throw new NotImplementedException();
        }

    }
}