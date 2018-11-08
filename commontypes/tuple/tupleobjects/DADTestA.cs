using System;

namespace CommonTypes.tuple.tupleobjects
{
    [Serializable] //TODO I've added this.. if its not allowed, we need to pass the tubles as the string representation of these classes
    public class DADTestA {
        public int i1;
        public string s1;

        public DADTestA(int pi1, string ps1) {
            i1 = pi1;
            s1 = ps1;
        }
        public bool Equals(DADTestA o) {
            if (o == null) {
                return false;
            } else {
                return ((this.i1 == o.i1) && (this.s1.Equals (o.s1)));
            }
        }
    }
}