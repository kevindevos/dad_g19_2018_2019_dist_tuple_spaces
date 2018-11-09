using CommonTypes;
using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ServerNamespace.XL {
    public class Worker : ITupleOperations {
        public TupleSpace TupleSpace; 

        public Worker(TupleSpace tupleSpace) {
            TupleSpace = tupleSpace;
        }

        public void Write(Tuple tuple) {
            TupleSpace.Write(tuple);
        }

        public List<Tuple> Read(TupleSchema tupleSchema) {
            return TupleSpace.Read(tupleSchema);
        }

        public List<Tuple> Take(TupleSchema tupleSchema) {
            return TupleSpace.Take(tupleSchema);
        }

        public void Remove(TupleSchema tupleSchema) {
            TupleSpace.Remove(tupleSchema);
        }
    }
}
