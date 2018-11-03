using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    class Animal : IEquatable<Animal> {
        public string name { get { return name; } set { name = value; } } 
        public int age { get { return age; } set { age = value;  } }

        public Animal(string name, int age) {
            this.name = name;
            this.age = age;
        }

        public bool Equals(Animal other) {
            return this.name.Equals(other.name)
                && this.age == other.age
                && this.GetType().Equals(other.GetType());
        }
    }
}
