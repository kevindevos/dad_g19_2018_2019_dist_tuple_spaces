using System.Collections.Generic;
using CommonTypes.tuple;

namespace CommonTypes.message {
    public class Response : Message {
        private List<Tuple> _tupleList;
        // The request to which the response refers to
        private Request _request;
        public List<Tuple> TupleList { get;  set; }
        public Request Request { get; private set; }

        public Response(Request request, List<Tuple> tupleList, string srcRemoteURL) : base (srcRemoteURL){
            Request = request;
            TupleList = tupleList;
        }
    }
}
