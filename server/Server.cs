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
        protected ServerBehaviour behaviour;

        // A dictionary containing the most recent sequence numbers of the most recent request of each client.
        protected Dictionary<string, int> mostRecentClientRequestSeqNumbers;

        // A queue (FIFO) of requests the server receives, mostly relevant for the master server, that decides which request to be executed first 
        protected Queue<Request> requestQueue;

        // Tuple space
        protected TupleSpace tupleSpace { get { return tupleSpace; } set { tupleSpace = value; } }


        static void Main(string[] args) {
            Server server = new Server();

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }

        public Server(){
            this.tupleSpace = new TupleSpace();
            this.behaviour = new ServerBehaviour();
        }
        

        public void upgradeToMaster() {
            this.behaviour = new MasterServerBehaviour();
        }

        public void downgradeToNormal() {
            this.behaviour = new ServerBehaviour();
        }

        


        

    }

}
