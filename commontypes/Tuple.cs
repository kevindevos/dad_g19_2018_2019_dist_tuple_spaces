using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes{
    public class Tuple{
        public readonly List<object> fields;

        public Tuple(List<object> fields) {
            this.fields = fields;
        }

        public int GetSize()
        {
            return fields.Count;
        }

        public override string ToString()
        {
            return "(" + String.Join(", ", fields.ToArray()) + ")";
        }
    }
}
