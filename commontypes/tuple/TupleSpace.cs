using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CommonTypes.tuple
{
    public class TupleSpace : ITupleOperations
    {
        // <logical name, list of tuples with that logical name>
        private readonly ConcurrentDictionary<object, List<Tuple>> _tupleSpace;
        private readonly ConcurrentDictionary<object, object> _tupleSpaceLocks;

        public TupleSpace()
        {
            _tupleSpace = new ConcurrentDictionary<object, List<Tuple>>();
            _tupleSpaceLocks = new ConcurrentDictionary<object, object>();

        }

        public void Write(Tuple tuple)
        {
            lock (_tupleSpaceLocks.GetOrAdd(tuple.LogicalName, new object()))
            {
                _tupleSpace.AddOrUpdate(tuple.LogicalName,
                                   valueToAdd => new List<Tuple> { tuple },
                                   (key, oldValue) => new List<Tuple>(oldValue) { tuple }
                                  );
            }
        }

        // TODO what if there is no tuple? how do we wait? above?
        public List<Tuple> Read(TupleSchema tupleSchema)
        {
            List<Tuple> tuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.LogicalName, new object()))
            {
                tuples = GetMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.LogicalName, new List<Tuple>()), tupleSchema);
            }

            return tuples;
        }

        public List<Tuple> Take(TupleSchema tupleSchema)
        {
            List<Tuple> matchingTuples;

            lock (_tupleSpaceLocks.GetOrAdd(tupleSchema.LogicalName, new object())) {
                
                // get matching tuples
                matchingTuples = GetMatchingTuples(_tupleSpace.GetOrAdd(tupleSchema.LogicalName, new List<Tuple>()), tupleSchema);
                var nonMatchingTuples = _tupleSpace.GetOrAdd(tupleSchema.LogicalName, 
                        new List<Tuple>()).Except(matchingTuples).ToList();
                
                // replace oldList with nonMatchingTuples
                _tupleSpace.AddOrUpdate(tupleSchema.LogicalName,
                    valueToAdd => nonMatchingTuples,
                    (key, oldValue) => new List<Tuple>(nonMatchingTuples)
                );
            }

            return matchingTuples;
        }

        // remove tuples , used for take's phase 2 in Xu and Liskov
        public List<Tuple> Remove(TupleSchema tupleSchema) {
            // TODO
            return null;
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
