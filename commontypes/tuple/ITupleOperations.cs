using System;
using System.Collections.Generic;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes
{
    public interface ITupleOperations
    {
        void Write(Tuple tuple);
        List<Tuple> Read(TupleSchema tupleSchema);
        List<Tuple> Take(TupleSchema tupleSchema);
    }
}
