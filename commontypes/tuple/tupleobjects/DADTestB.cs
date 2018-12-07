using System;
using System.Collections.Generic;

namespace CommonTypes.tuple.tupleobjects
{
    [Serializable] //TODO I've added this.. if its not allowed, we need to pass the tubles as the string representation of these classes
    public class DADTestB {
        public int i1;
        public string s1;
        public int i2;

        public DADTestB(int pi1, string ps1, int pi2) {
            i1 = pi1;
            s1 = ps1;
            i2 = pi2;
        }

        public bool Equals(DADTestB o) {
            if (o == null) {
                return false;
            } else {
                return ((this.i1 == o.i1) && (this.s1.Equals (o.s1)) && (this.i2 == o.i2));
            }
        }
        
        public override bool Equals(Object obj) {
            if (obj == null || obj.GetType() != typeof(DADTestB)) {
                return false;
            } else
            {
                DADTestB o = (DADTestB) obj;
                return i1 == o.i1 && s1.Equals(o.s1) && i2 == o.i2;
            }
        }
        
        public override string ToString()
        {
            return "DADTestB(" + i1 + ", " + s1 + ", " + i2 + ")" ;
        }

    }
}