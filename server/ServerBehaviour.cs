using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace {
    public class ServerBehaviour {
        private Server server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            this.server = server;
        }

        protected virtual List<object> Read(List<object> members) {
            // find all tuples that contain all objects in members 
            // return 


            return null;
        }

        // TODO , this should be called when message is received, to diverge actions depending on type of request, and if from master or not (entry point)
        protected virtual void OnReceiveRequest() { 

        }

    }
}
