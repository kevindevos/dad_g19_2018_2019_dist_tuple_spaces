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
    public class Server : IServer {

        private TupleSpace tupleSpace;

        public Server(){
            tupleSpace = new TupleSpace();
        }
        

    }

}
