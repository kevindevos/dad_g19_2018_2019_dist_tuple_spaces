using CommonTypes.tuple;

namespace CommonTypes.message{
    [System.Serializable]
    public class WriteRequest : Request{
        public WriteRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple) { }
    }
}