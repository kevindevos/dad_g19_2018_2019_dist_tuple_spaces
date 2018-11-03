using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    public class Response : Message {
        List<Tuple> tupleList;

        public Response(int seqNum, string clientRemoteUrl, List<Tuple> tupleList) : base(seqNum, clientRemoteUrl, null) {
            this.tupleList = tupleList;
        }
    }
}
