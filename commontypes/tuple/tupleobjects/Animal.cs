using CommonTypes.domain;

namespace CommonTypes {
    class Animal : ITupleObject {
        public string name { get { return name; } set { name = value; } } 
        public int age { get { return age; } set { age = value;  } }

        public Animal(string name, int age) {
            this.name = name;
            this.age = age;
        }

        public new bool Equals(object other)
        {
            // if other is subclass of Animal or is instance of Animal, then compare fields
            if ( other.GetType().IsSubclassOf(typeof(Animal)) || other.GetType().Equals(typeof(Animal))) {
                Animal otherAnimal = (Animal)other;

                return name.Equals(otherAnimal.name) && age == otherAnimal.age;
            }

            return false;
        }
    }
}
