using System;
namespace CommonTypes
{
    public interface ITupleOperations
    {
        void Write(Tuple tuple);
        Tuple Read(TupleSchema tupleSchema);
        Tuple Take(TupleSchema tupleSchema);
    }
}
