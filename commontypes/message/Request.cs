using CommonTypes.message;
using CommonTypes.tuple;

namespace CommonTypes {

    public enum RequestType { READ, WRITE, TAKE }

    [System.Serializable]
    public class Request : Message {
        
        public RequestType RequestType { get; }
        public int SeqNum { get; }
        public Tuple Tuple { get; }
        public string SrcEndpointURL { get; }
        
        public Request(int seqNum, string srcEndpointURL, RequestType requestType, Tuple tuple) : base(srcEndpointURL)
        {
            RequestType = requestType;
            SeqNum = seqNum;
            Tuple = tuple;
            SrcEndpointURL = srcEndpointURL;
        }

    }
}
