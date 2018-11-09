using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientXL : Client {
        public List<RemotingEndpoint> View { get { return KnownServerRemotes; } } // change name for XL

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
        }

        public ClientXL(string host, int port) : base(host, port) {
        }

       
        public override void Write(Tuple tuple) {
            // TODO remote exceptions?
            var request = new Request(ClientRequestSeqNumber, EndpointURL, RequestType.WRITE, tuple);

            Console.WriteLine("Sending request with sequence number: " + request.SeqNum);
            SendMessageToView(request);
            ClientRequestSeqNumber++;
        }

        private void SendMessageToView(Request request) {
            SendMessageToRemotes(KnownServerRemotes, request);
        }

        public override Tuple Read(Tuple tuple) {
            return null;
        }

        public override Tuple Take(Tuple tuple) {
            return null;
        }
    }
}
