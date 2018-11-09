using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CommonTypes.tuple
{
    public class TupleSpace : ITupleOperations
    {
        // <size of tuple, list of tuples with that size>
        private readonly ConcurrentDictionary<int, List<Tuple>> _tupleSpace;
        private readonly ConcurrentDictionary<int, object> _tupleSpaceLocks;

        public TupleSpace()
        {
            _tupleSpace = new ConcurrentDictionary<int, List<Tuple>>();
            _tupleSpaceLocks = new ConcurrentDictionary<int, object>();

        }

        public void Write(Tuple tuple)
        {
            lock (_tupleSpaceLocks.GetOrAdd(tuple.GetSize(), new Object()))
            {
                _tupleSpace.AddOrUpdate(tuple.GetSize(),
                                   valueToAdd => new List<Tuple> { tuple },
                                   (key, oldValue) => new List<Tuple>(oldValue) { tuple }
                                  );
            }
        }

        // TODO what if there is no tuple? how do we wait? above?
        public List<Tuple> Read(TupleSchema tupleSchema)
        {
            List<Tuple> matchingTuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.Schema.GetSize(), new object()))
            {
                matchingTuples = GetMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.Schema.GetSize(), new List<Tuple>()), tupleSchema);
            }

            return matchingTuples;
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            List<Tuple> matchingTuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.Schema.GetSize(), new object())) {
                matchingTuples = GetAndRemoveMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.Schema.GetSize(), new List<Tuple>()), tupleSchema);
            }

            return matchingTuples;
        }

        private List<Tuple> GetMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.FindAll(tupleSchema.Match);
        }

        private List<Tuple> GetAndRemoveMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema) {
            List<Tuple> tuples = GetMatchingTuples(listOfTuples, tupleSchema);
            foreach (Tuple tuple in tuples) {
                listOfTuples.Remove(tuple);
            }
            return listOfTuples;
        }

        // TODO can we guarantee that this returns deterministically always the same value?
        private Tuple GetFirstMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.Find(tupleSchema.Match);
        }

    }
}
