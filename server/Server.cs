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
        private TupleSpace tupleSpace;

        static void Main(string[] args) {
            Server server = new Server();

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }

        public Server(){
            tupleSpace = new TupleSpace();
        }


        public object Read(List<object> members) {
            // find all tuples that contain all objects in members 
            // return 


            return null;
        }

        void OnReceiveRequest() { // TODO , this should be called when message is received, to diverge actions depending on type of request, and if from master or not (entry point)

        }


        

    }

}
