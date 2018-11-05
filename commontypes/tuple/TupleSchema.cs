using CommonTypes.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonTypes
{
    // a "pattern" to match against, consists of a list of fields that have to be all contained in a tuple for there to be a match
    
    public class TupleSchema
    {
        public Tuple schema { get { return schema; } set { schema = value; } }

        public TupleSchema(Tuple tuple)
        {
            this.schema = tuple;
        }


        public Boolean Match(Tuple tuple)
        {
            // if there are more required tuple members than they exist, return false
            if (tuple.GetSize() < schema.GetSize())
                return false;

            for (int i = 0; i < schema.GetSize(); i++) {
                for (int j = 0; j < tuple.GetSize(); j++) {
                    object schemaField = schema.fields[i];
                    object tupleField = tuple.fields[j];

                    // check type
                    if (!schemaField.GetType().Equals(tupleField.GetType()))
                        return false;

                    // check string match
                    if (Regex.IsMatch(tupleField.ToString(), WildCardToRegular(schemaField.ToString())))
                        return true;

                    // TODO ( only works for ITupleObject instances ? )
                    // check for object match , and numbers match
                    if (tupleField.Equals(schemaField)) {
                        return true;
                    }

                }
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
