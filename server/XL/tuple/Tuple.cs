using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNamespace.XL.tuple {
    public class Tuple : CommonTypes.tuple.Tuple {

        public Tuple(List<object> fields) : base(fields) {
        }

        public object LogicalName { get { return Fields[0]; } set { Fields[0] = value; } }

    }
}
