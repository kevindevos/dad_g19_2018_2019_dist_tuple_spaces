using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    public enum RequestType { READ, WRITE, TAKE } // put? write is put? 

    public class Request : Message {
        public RequestType requestType { get { return requestType;  } set { requestType = value; } }

        public Request(int seqNum, string clientRemoteURL, RequestType rt, Tuple tuple) : base(seqNum, clientRemoteURL, tuple) {
            this.requestType = rt;
        }
    }
}
