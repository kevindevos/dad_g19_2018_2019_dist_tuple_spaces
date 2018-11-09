using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace.XL {
    public class Worker {
        public TupleSpace TupleSpace; 

        public Worker(TupleSpace tupleSpace) {
            TupleSpace = tupleSpace;
        }
    }
}
