using System.Text.RegularExpressions;

namespace CommonTypes.tuple
{
    // a "pattern" to match against, consists of a list of fields that have to be all contained in a tuple for there to be a match
    
    public class TupleSchema
    {
        public readonly Tuple Schema;

        public TupleSchema(Tuple tuple)
        {
            Schema = tuple;
        }


        public bool Match(Tuple tuple)
        {
            // if there are more required tuple members than they exist, return false
            if (tuple.GetSize() < Schema.GetSize())
                return false;

            for (var i = 0; i < Schema.GetSize(); i++) {
                for (var j = 0; j < tuple.GetSize(); j++) {
                    var schemaField = Schema.Fields[i];
                    var tupleField = tuple.Fields[j];

                    // check type
                    if (schemaField.GetType() != tupleField.GetType())
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
        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }
    }
}
