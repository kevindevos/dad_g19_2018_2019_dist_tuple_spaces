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

        public List<Tuple> Read(TupleSchema tupleSchema) {
            throw new NotImplementedException(); //TODO
        }

        public List<Tuple> Take(TupleSchema tupleSchema) {
            throw new NotImplementedException(); //TODO
        }

        public void Write(Tuple tuple) {
            throw new NotImplementedException(); //TODO
        }
    }
}
