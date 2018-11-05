using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes.message;

namespace CommonTypes {
    public enum RequestType { READ, WRITE, TAKE }  


    public abstract class Request : Message {
        private int _seqNum;
        private string srcRemoteUrl;
        private RequestType _requestType;
        private Tuple _tuple;

        public Request(int seqNum, string clientRemoteUrl, RequestType requestType, Tuple tuple)
        {
            _requestType = requestType;
            _seqNum = seqNum;
            srcRemoteUrl = clientRemoteUrl;
            _tuple = tuple;
        }

        public RequestType RequestType { get; private set; }
        public int SeqNum { get; private set; }
        public string SrcRemoteURL { get; private set; }
        public Tuple Tuple { get; private set; }

        
    }

    public class ReadRequest : Request {
        public ReadRequest(int seqNum, string clientRemoteURL, Tuple tuple) : base(seqNum, clientRemoteURL, RequestType.READ, tuple) { }
    }
    public class WriteRequest : Request {
        public WriteRequest(int seqNum, string clientRemoteURL, Tuple tuple) : base(seqNum, clientRemoteURL, RequestType.WRITE, tuple) { }
    }
    public class TakeRequest : Request {
        public TakeRequest(int seqNum, string clientRemoteURL, Tuple tuple) : base(seqNum, clientRemoteURL, RequestType.TAKE, tuple) { }
    }
}
