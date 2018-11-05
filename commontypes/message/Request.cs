using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    public enum RequestType { READ, WRITE, TAKE }  

    public abstract class Request : Message {
        private RequestType requestType;
        private int seqNum;
        private string clientRemoteURL;
        private Tuple tuple;

        public RequestType RequestType { get; private set; }
        public int SeqNum { get; private set; }
        public string ClientRemoteURL { get; private set; }
        public Tuple Tuple { get; private set; }

        public Request(int seqNum, string clientRemoteURL, RequestType rt, Tuple tuple) {
            this.SeqNum = seqNum;
            this.ClientRemoteURL = clientRemoteURL;
            this.RequestType = rt;
            this.Tuple = tuple;
        }
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
