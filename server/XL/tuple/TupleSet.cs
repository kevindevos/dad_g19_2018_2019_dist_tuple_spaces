using CommonTypes.tuple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace.XL.tuple {
    // a partition of tuples based on their associated logical name ( first field in tuple )
    public class TupleSet  {
        public List<Tuple> Tuples;
        public object AssociatedLogicalName;

        public TupleSet(object associatedLogicalName, List<Tuple> tuples) {
            AssociatedLogicalName = associatedLogicalName;
            Tuples = tuples;
        }
    }
}
