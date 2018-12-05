using CommonTypes.tuple;

namespace CommonTypes.message{
    [System.Serializable]
    public class TakeRequest : Request{
        public TakeRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple){ }
    }
}