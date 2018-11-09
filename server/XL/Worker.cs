using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace.XL {
    public class Worker {
        private TupleSpace _tupleSet;
        public TupleSpace TupleSet; // local worker's tuple set

        public Worker(TupleSpace TupleSet) {
            TupleSet = TupleSet;
        }
    }
}
