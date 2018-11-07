using CommonTypes.message;
using CommonTypes.tuple;

namespace CommonTypes {
    public enum RequestType { READ, WRITE, TAKE }  


    public abstract class Request : Message {
        private int _seqNum;
        private RequestType _requestType;
        private Tuple _tuple;
        private string _srcEndpointURL;

        public Request(int seqNum, string srcEndpointURL, RequestType requestType, Tuple tuple) : base(srcEndpointURL)
        {
            _requestType = requestType;
            _seqNum = seqNum;
            _tuple = tuple;
            _srcEndpointURL = srcEndpointURL;
        }

        public RequestType RequestType { get; private set; }
        public int SeqNum { get; private set; }
        public Tuple Tuple { get; private set; }
        public string SrcEndpointURL { get; private set; }

        
    }

    public class ReadRequest : Request {
        public ReadRequest(int seqNum, string srcRemoteURL, Tuple tuple) : base(seqNum, srcRemoteURL, RequestType.READ, tuple) { }
    }
    public class WriteRequest : Request {
        public WriteRequest(int seqNum, string srcRemoteURL, Tuple tuple) : base(seqNum, srcRemoteURL, RequestType.WRITE, tuple) { }
    }
    public class TakeRequest : Request {
        public TakeRequest(int seqNum, string srcRemoteURL, Tuple tuple) : base(seqNum, srcRemoteURL, RequestType.TAKE, tuple) { }
    }
}
