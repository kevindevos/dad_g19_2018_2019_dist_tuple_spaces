using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public interface ITupleOperations
    {
        void Write(Tuple tuple);
        List<Tuple> Read(TupleSchema tupleSchema);
        List<Tuple> Take(TupleSchema tupleSchema);
    }
}
