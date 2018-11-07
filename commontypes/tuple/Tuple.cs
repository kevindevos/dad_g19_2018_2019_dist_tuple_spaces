using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.tuple{
    public class Tuple{
        public readonly List<object> Fields;

        public Tuple(List<object> fields) {
            Fields = fields;
        }

        public int GetSize()
        {
            return Fields.Count;
        }

        public bool Contains(object member) {
            return Fields.Contains(member);
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", Fields.ToArray()) + ")";
        }
    }
}
