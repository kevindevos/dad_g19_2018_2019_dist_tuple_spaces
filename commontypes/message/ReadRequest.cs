using System;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes.message{
    [Serializable]
    public class ReadRequest : Request{
        public ReadRequest(int seqNum, string srcEndpointURL, Tuple tuple) : base(seqNum, srcEndpointURL, tuple) { }
    }
}