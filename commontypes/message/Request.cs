using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    public enum RequestType { READ, WRITE, TAKE, PUT }

    public class Request : Message {
        public RequestType requestType { get { return requestType;  } set { requestType = value; } }

        public Request(int seqNum, int clientId, RequestType rt, string data) : base(seqNum, clientId, data) {
            this.requestType = rt;
        }
    }
}
