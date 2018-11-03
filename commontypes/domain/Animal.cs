using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    class Animal : IEquatable<object> {
        public string name { get { return name; } set { name = value; } } 
        public int age { get { return age; } set { age = value;  } }



        public Animal(string name, int age) {
            this.name = name;
            this.age = age;
        }

        public bool Equals(object other) {
            // if other is subclass of Animal or is instance of Animal, then compare fields
            if ( other.GetType().IsSubclassOf(typeof(Animal)) || other.GetType().Equals(typeof(Animal))) {
                Animal otherAnimal = (Animal)other;

                return this.name.Equals(otherAnimal.name) && this.age == otherAnimal.age;
            }
            else {
                return false;
            }
        }
    }
}
