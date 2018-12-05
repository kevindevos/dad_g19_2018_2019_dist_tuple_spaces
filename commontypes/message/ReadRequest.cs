using CommonTypes.tuple;

namespace CommonTypes.message{
    [System.Serializable]
    public class ReadRequest : Request{
        public ReadRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple) { }
    }
}