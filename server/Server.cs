using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace{
    public class Server {
        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        public ServerBehaviour behaviour;

        // A dictionary containing the most recent sequence numbers of the most recent request of each client.
        public Dictionary<int, int> mostRecentClientRequestSeqNumbers;

        // A queue (FIFO) of requests the server receives, mostly relevant for the master server, that decides which request to be executed first 
        public Queue<Request> requestQueue;

        // Tuple space
        public TupleSpace tupleSpace { get { return tupleSpace; } set { tupleSpace = value; } }


        static void Main(string[] args) {
            Server server = new Server();

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }

        public Server(){
            this.tupleSpace = new TupleSpace();
            this.behaviour = new ServerBehaviour(this);
        }
        

        public void upgradeToMaster() {
            this.behaviour = new MasterServerBehaviour(this);
        }

        public void downgradeToNormal() {
            this.behaviour = new ServerBehaviour(this);
        }

        public void OnReceiveMessage(Message message) {
            behaviour.OnReceiveMessage(message);
        }


        

    }

}
