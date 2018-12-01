using System;

namespace CommonTypes.tuple.tupleobjects
{
    [Serializable] //TODO I've added this.. if its not allowed, we need to pass the tubles as the string representation of these classes
    public class DADTestC {
        public int i1;
        public string s1;
        public string s2;

        public DADTestC(int pi1, string ps1, string ps2) {
            i1 = pi1;
            s1 = ps1;
            s2 = ps2;
        }

        public bool Equals(DADTestC o) {
            if (o == null) {
                return false;
            } else {
                return ((this.i1 == o.i1) && (this.s1.Equals (o.s1)) && (this.s2.Equals (o.s2)));
            }
        }
        
        public override bool Equals(Object obj) {
            if (obj == null || obj.GetType() != typeof(DADTestC)) {
                return false;
            } else
            {
                DADTestC o = (DADTestC) obj;
                return ((this.i1 == o.i1) && (this.s1.Equals (o.s1)) && (this.s2.Equals (o.s2)));
            }
        }
        
        public override string ToString()
        {
            return "DADTestC(" + i1 + ", " + s1 + ", " + s2 + ")" ;
        }
    }
}