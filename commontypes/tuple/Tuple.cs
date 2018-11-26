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

        //TODO review
        public override bool Equals(object obj)
        {
            if(obj != null && 
                   obj.GetType() == GetType())
            {
                Tuple t;
                t = (Tuple) obj;
                if (t.GetSize() == GetSize())
                {
                    for (int i = 0; i < GetSize(); i++)
                    {
                        if (!Fields[i].Equals(t.Fields[i]))
                            return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
