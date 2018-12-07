using System;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes.message{
    [Serializable]
    public class WriteRequest : Request{
        public WriteRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple) { }
    }
}