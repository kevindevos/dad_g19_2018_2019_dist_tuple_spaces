using CommonTypes;
using CommonTypes.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNamespace {
    class Client : IRemoting {
        private int serverPort;
        public int clientPort;

        static void Main(string[] args) {
            Client client = new Client();
        }

        public Client() { }

        //TODO where does the client find out which destination port(s) to use?
        // maybe method in IRemoting, fetch available servers?

        public IRemoting connect(string url, int destPort, string objIdentifier) {
            IRemoting remote = (IRemoting)Activator.GetObject(
                typeof(IRemoting),
                "tcp://" + url + ":" + destPort + "/" + objIdentifier);

            return remote;
        }

        public void OnReceiveMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
