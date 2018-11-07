namespace CommonTypes.message {
    // An order is a message sent by a master to other normal servers, to perform a certain action/request
    [System.Serializable]
    public class Order : Message {
        private Request _request;
        private int _seqNum;

        public int SeqNum { get; private set; }
        public Request Request { get; private set; }

        public Order(Request request, int orderSequenceNumber, string srcRemoteURL) : base(srcRemoteURL){
            Request = request;
            SeqNum = orderSequenceNumber;
        }

    }
}
