using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class TupleSchema
    {
        public readonly Tuple schema;

        public TupleSchema(Tuple tuple)
        {
            this.schema = tuple;
        }

        public Boolean Match(Tuple tuple)
        {
            if (tuple.GetSize() != schema.GetSize())
                return false;


            for (int i = 0; i < schema.GetSize(); i++)
            {
                object schemaField = schema.fields[0];
                object tupleField = tuple.fields[0];

                // check type
                if (!schemaField.GetType().Equals(tupleField.GetType()))
                    return false;

                // check string match
                if (!Regex.IsMatch(tupleField.ToString(), WildCardToRegular(schemaField.ToString())))
                    return false;

                //TODO compare two objects like, MyClass(1, "a") with MyClass, or with null, etc

            }

            return true;
        }

        // https://stackoverflow.com/questions/30299671/matching-strings-with-wildcard
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }
    }
}
