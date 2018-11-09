using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientXL : Client {
        public ClientXL() : base(DefaultClientHost, DefaultClientPort) {
        }

        public ClientXL(string host, int port) : base(host, port) {
        }

        public override Message OnReceiveMessage(Message message) {
            throw new NotImplementedException();
        }

        public override Message OnSendMessage(Message message) {
            throw new NotImplementedException();
        }


        public override void Write(Tuple tuple) {
            
        }

        public override Tuple Read(Tuple tuple) {
            return null;
        }

        public override Tuple Take(Tuple tuple) {
            return null;
        }
    }
}
