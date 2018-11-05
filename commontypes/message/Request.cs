using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes.message;

namespace CommonTypes {
    public enum RequestType { Read, Write, Take }  

    public class Request : Message {
        private int _seqNum;
        private string _clientRemoteUrl;
        private RequestType _requestType;
        private Tuple _tuple;

        public Request(int seqNum, string clientRemoteUrl, RequestType requestType, Tuple tuple)
        {
            _requestType = requestType;
            _seqNum = seqNum;
            _clientRemoteUrl = clientRemoteUrl;
            _tuple = tuple;
        }

        public RequestType RequestType { get; private set; }
        public int SeqNum { get; private set; }
        public string ClientRemoteUrl { get; private set; }
        public Tuple Tuple { get; private set; }

        
    }
}
