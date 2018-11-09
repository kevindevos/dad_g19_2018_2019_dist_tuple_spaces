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

        public ClientXL() : this(DefaultClientHost, DefaultClientPort) {
            SendMessageDel = new SendMessageDelegate(SendMessageToView);
        }

        public ClientXL(string host, int port) : base(host, port) {
        }

        private Message SendMessageToView(Message message) {
            SendMessageToRemotes(KnownServerRemotes, message);
            return null;
        }

    }
}
