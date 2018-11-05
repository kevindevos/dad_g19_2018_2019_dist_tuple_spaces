using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    public class Response : Message {
        private List<Tuple> tupleList;
        // The request to which the response refers to
        private Request request;
        

        public List<Tuple> TupleList { get; private set; }
        public Request Request { get; private set; }

        public Response(Request request, List<Tuple> tupleList, string srcRemoteURL) : base (srcRemoteURL){
            this.Request = request;
            this.TupleList = tupleList;
        }
    }
}
