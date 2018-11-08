using CommonTypes.message;
using CommonTypes.tuple;

namespace CommonTypes {

    public enum RequestType { READ, WRITE, TAKE }

    [System.Serializable]
    public class Request : Message {
        private int _seqNum;
        private RequestType _requestType;
        private Tuple _tuple;
        private string _srcEndpointURL;

        public Request(int seqNum, string srcEndpointURL, RequestType requestType, Tuple tuple) : base(srcEndpointURL)
        {
            RequestType = requestType;
            SeqNum = seqNum;
            Tuple = tuple;
            SrcEndpointURL = srcEndpointURL;
        }

        public RequestType RequestType { get; private set; }
        public int SeqNum { get; private set; }
        public Tuple Tuple { get; private set; }
        public string SrcEndpointURL { get; private set; }

        
    }
}
