using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace {
    class MasterServerBehaviour : ServerBehaviour {

        public MasterServerBehaviour(Server server) : base(server) {

        }

        protected override List<object> Read(List<object> objects) {
            // do master stuff


            
            return base.Read(objects);
        }

        protected override void OnReceiveRequest() {
            // do master stuff like deciding the request to execute by all servers for example

            base.OnReceiveRequest();
        }
    }
}
    