using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace CommonTypes
{
    public class TupleSpace : ITupleOperations
    {
        // <size of tuple, list of tuples with that size>
        private ConcurrentDictionary<int, List<Tuple>> tupleSpace;
        private ConcurrentDictionary<int, object> tupleSpaceLocks;

        public TupleSpace()
        {
            tupleSpace = new ConcurrentDictionary<int, List<Tuple>>();
            tupleSpaceLocks = new ConcurrentDictionary<int, object>();

        }

        public void Write(Tuple tuple)
        {
            // TODO better alternative?
            lock (tupleSpaceLocks.GetOrAdd(tuple.GetSize(), new Object()))
            {
                tupleSpace.AddOrUpdate(tuple.GetSize(),
                                   (valueToAdd) => new List<Tuple>() { tuple },
                                   (key, oldValue) => new List<Tuple>(oldValue) { tuple }
                                  );
            }
        }

        // TODO what if there is no tuple? how do we wait? above?
        public List<Tuple> Read(TupleSchema tupleSchema)
        {
            List<Tuple> matchingTuples;

            // TODO better alternative?
            lock (tupleSpaceLocks.GetOrAdd(tupleSchema.schema.GetSize(), new object()))
            {
                matchingTuples = GetMatchingTuples(tupleSpace.GetOrAdd(tupleSchema.schema.GetSize(), new List<Tuple>()), tupleSchema);
            }

            return matchingTuples;
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            throw new NotImplementedException();
        }

        private List<Tuple> GetMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.FindAll(tupleSchema.Match);
        }

        // TODO can we guarantee that this returns deterministically always the same value?
        private Tuple GetFirstMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.Find(tupleSchema.Match);
        }

    }
}
