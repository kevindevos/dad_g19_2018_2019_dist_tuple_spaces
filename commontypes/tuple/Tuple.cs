using System.Collections.Generic;

namespace CommonTypes.tuple{
    [System.Serializable]
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
