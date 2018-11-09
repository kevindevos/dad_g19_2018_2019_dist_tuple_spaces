using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientXL : Client {
        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
        }

        public ClientXL(string host, int port) : base(host, port) {
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
