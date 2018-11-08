using System.Collections.Generic;
using CommonTypes.tuple;

namespace CommonTypes.message {
    [System.Serializable]
    public class Response : Message {
        private List<Tuple> _tupleList;
        public List<Tuple> TupleList { get; set; }

        // The request to which the response refers to
        private Request _request;
        public Request Request { get; private set; }

        public Response(Request request, List<Tuple> tupleList, string srcRemoteURL) : base (srcRemoteURL){
            Request = request;
            TupleList = tupleList;
        }
    }
}
