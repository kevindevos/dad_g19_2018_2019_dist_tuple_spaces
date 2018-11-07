using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes {

    public enum RequestType { READ, WRITE, TAKE }

    [System.Serializable]
    public class Request : Message {
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
}
