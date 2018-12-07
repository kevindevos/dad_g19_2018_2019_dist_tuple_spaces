using System;
using System.Collections.Generic;

namespace CommonTypes.tuple{
    [Serializable]
    public class Tuple : IEquatable<Tuple>{
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

        public bool Equals(Tuple obj){
            if(obj != null && obj.GetType() == GetType())
            {
                if (obj.GetSize() == GetSize())
                {
                    for (int i = 0; i < GetSize(); i++)
                    {
                        if (!Fields[i].Equals(obj.Fields[i]))
                            return false;
                    }
                }
                else{
                    return false;
                }

                return true;
            }

            return false;
        }

        public override bool Equals(object obj){
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tuple) obj);
        }

        // for example Intersect to compare tuples it does first t1.gethash == t2.getHash && t1.equals(t2)
        // we set it to return the same value every time, to just use equals
        // because for some reason tuples are giving diferent hashes for the same value??
        public override int GetHashCode(){
            return 0; 
        }
    }
}
