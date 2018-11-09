using System;
using CommonTypes;
using CommonTypes.message;
using CommonTypes.tuple;

namespace ServerNamespace.Behaviour {
    public abstract class ServerBehaviour {
        protected readonly Server Server; // keep instance of the server for accessing things like sequence numbers, request queue, tuple space

        public ServerBehaviour(Server server) {
            Server = server;
        }

        public abstract Message ProcessMessage(Message message);
        public abstract Response PerformRequest(Request request);


    }
}
