using System.Collections.Generic;
using CommonTypes.tuple;

namespace CommonTypes.message {
    [System.Serializable]
    public class Response : Message {
        private Tuple _tuple;
        public List<Tuple> Tuples { get; set; }

        // The request to which the response refers to
        private Request _request;
        public Request Request { get; private set; }

        public Response(Request request, List<Tuple> tuples, string srcRemoteURL) : base (srcRemoteURL){
            Request = request;
            Tuples = tuples;
        }
    }
}
