﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
            lock (_tupleSpaceLocks.GetOrAdd(tuple.GetSize(), new object()))
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
            List<Tuple> tuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.Schema.GetSize(), new object()))
            {
                tuples = GetMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.Schema.GetSize(), new List<Tuple>()), tupleSchema);
            }

            return tuples;
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            List<Tuple> matchingTuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.Schema.GetSize(), new object()))
            {

                // get matching tuples
                matchingTuples = GetMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.Schema.GetSize(), new List<Tuple>()), tupleSchema);
                var nonMatchingTuples = _tupleSpace.GetOrAdd(tupleSchema.Schema.GetSize(),
                     new List<Tuple>()).Except(matchingTuples).ToList();

                // replace oldList with nonMatchingTuples
                _tupleSpace.AddOrUpdate(tupleSchema.Schema.GetSize(),
                    valueToAdd => nonMatchingTuples,
                    (key, oldValue) => new List<Tuple>(nonMatchingTuples)
                );
            }

            return matchingTuples;

        }

        public Dictionary<int, List<Tuple>> GetCopy()
        {
            Dictionary<int, List<Tuple>> copy;
            
            lock (_tupleSpace)
            {
                copy = new Dictionary<int, List<Tuple>>(_tupleSpace);                
            }

            return copy;
        }

        private List<Tuple> GetMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.FindAll(tupleSchema.Match);
        }

        private Tuple GetMatchingTuple(List<Tuple> listOfTuples, TupleSchema tupleSchema) {
            return listOfTuples.FindAll(tupleSchema.Match).First();
        }

        // TODO can we guarantee that this returns deterministically always the same value?
        private Tuple GetFirstMatchingTuples(List<Tuple> listOfTuples, TupleSchema tupleSchema)
        {
            return listOfTuples.Find(tupleSchema.Match);
        }

    }
}
