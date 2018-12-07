using System;
using System.Collections.Generic;

namespace CommonTypes.tuple.tupleobjects
{
    [Serializable] //TODO I've added this.. if its not allowed, we need to pass the tubles as the string representation of these classes
    public class DADTestA  {
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
        
        public override bool Equals(Object obj) {
            if (obj == null || obj.GetType() != typeof(DADTestA)) {
                return false;
            } else
            {
                DADTestA o = (DADTestA) obj;
                return i1 == o.i1 && s1.Equals(o.s1);
            }
        }

        public override string ToString()
        {
            return "DADTestA(" + i1 + ", " + s1 + ")" ;
        }
    }
}