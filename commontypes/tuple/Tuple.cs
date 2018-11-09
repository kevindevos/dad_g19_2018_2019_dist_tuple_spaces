using System.Collections.Generic;

namespace CommonTypes.tuple{
    [System.Serializable]
    public class Tuple{
        public readonly List<object> Fields;

        public object LogicalName { get { return Fields[0]; } set { Fields[0] = value; } }

        public Tuple(List<object> fields) {
            Fields = fields;
        }

        public int GetSize()
        {
            return Fields.Count;
        }

        public override string ToString()
        {
            var listWithNull = new List<object>();
            foreach (var field in Fields)
            {
                listWithNull.Add(field ?? "null");
            }
            return "(" + string.Join(", ", listWithNull.ToArray()) + ")";
        }
    }
}
