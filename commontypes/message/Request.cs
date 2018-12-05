using CommonTypes.tuple;

namespace CommonTypes.message {

    [System.Serializable]
    public abstract class Request : Message {
        
        public int SeqNum { get; }
        public Tuple Tuple { get; }
        
        protected Request(int seqNum, string srcEndpointURL, Tuple tuple) : base(srcEndpointURL)
        {
            SeqNum = seqNum;
            Tuple = tuple;
        }
        
    }
}
