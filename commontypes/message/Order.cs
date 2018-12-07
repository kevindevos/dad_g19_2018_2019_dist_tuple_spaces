using System;

namespace CommonTypes.message {
    // An order is a message sent by a master to other normal servers, to perform a certain action/request
    [Serializable]
    public class Order : Message {

        public int SeqNum { get; private set; }
        public Request Request { get; private set; }

        public Order(Request request, int orderSequenceNumber, string srcRemoteURL) : base(srcRemoteURL) {
            Request = request;
            SeqNum = orderSequenceNumber;
        }
    }
}
