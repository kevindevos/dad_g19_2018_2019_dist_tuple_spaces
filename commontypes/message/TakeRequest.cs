using System;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes.message{
    [Serializable]
    public class TakeRequest : Request{
        public TakeRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple){ }
    }
}