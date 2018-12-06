using System.Collections;
using System.Collections.Generic;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;

namespace Tests {
    
    [TestFixture]
    public class TupleSpaceTest
    {
        private TupleSpace _tupleSpace;
        public static readonly Tuple Tuple1 = new Tuple(new List<object> { "field1" });
        private static readonly Tuple Tuple2 = new Tuple(new List<object> { new DADTestA(1, "2") });
        private static readonly Tuple Tuple3= new Tuple(new List<object> { "field1", new DADTestB(1, "field2", 3), "field3" });
        private TupleSchema _tuple1Schema;
        private TupleSchema _tuple2Schema;
        private TupleSchema _tuple3Schema;
        
        [SetUp]
        public void Init()
        {
            _tupleSpace = new TupleSpace();
            _tuple1Schema = new TupleSchema(Tuple1);
            _tuple2Schema = new TupleSchema(Tuple2);
            _tuple3Schema = new TupleSchema(Tuple3);
            
        }

        [TearDown]
        public void Dispose()
        {
            
        }
       
        private class TupleDataClass
        {
            public static IEnumerable Tuples
            {
                get
                {
                    yield return new TestCaseData(Tuple1);
                    yield return new TestCaseData(Tuple2);
                    yield return new TestCaseData(Tuple3);
                }
            }  
        }
        
        /*
         * test the write
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Write1(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
        }
        
        /*
         * test the read without matching
         * should return an empty list
         */
        [Test]
        public void Read0()
        {
            var result = _tupleSpace.Read(_tuple1Schema);
            Assert.IsEmpty(result);
        }
        
        /*
         * test the read using a perfect match schema
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Read1(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
            var tupleSchema = new TupleSchema(tuple);
            var result = _tupleSpace.Read(tupleSchema);
            Assert.Contains(tuple, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        
        /*
         * write, read
         * should return the written value
         */
        private void ReadWildcard1_0(TupleSchema tupleSchemaWildcard)
        {
            _tupleSpace.Write(Tuple1);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple1, result);
            Assert.IsTrue(result.Count == 1);
        }

        /*
         * test *end wildcard
         */
        [Test]
        public void ReadWildcard1_1()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*field1"}));
            ReadWildcard1_0(tupleSchemaWildcard);
        }
        
        /*
         * test start* wildcard
         */
        [Test]
        public void ReadWildcard1_2()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"field1*"}));
            ReadWildcard1_0(tupleSchemaWildcard);
        }
        
        /*
         * test start* wildcard, without the full word
         */
        [Test]
        public void ReadWildcard1_3()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"fi*"}));
            ReadWildcard1_0(tupleSchemaWildcard);
        }
        
        /*
         * test *end wildcard, without the ful word
         */
        [Test]
        public void ReadWildcard1_4()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*d1"}));
            ReadWildcard1_0(tupleSchemaWildcard);
        }
        
        /*
         * test * wildcard
         */
        [Test]
        public void ReadWildcard1_5()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*"}));
            ReadWildcard1_0(tupleSchemaWildcard);
            
        }
        
        /*
         * write object
         * read using name of object type as wildcard
         * should return the written object
         */
        [Test]
        public void ReadWildcard2_1()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"DADTestA"}));
            
            _tupleSpace.Write(Tuple2);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple2, result);
        }
        
        /*
         * write object
         * read using null wildcard
         * should return the written object
         */
        [Test]
        public void ReadWildcard2_2()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {null}));
            
            _tupleSpace.Write(Tuple2);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple2, result);
        }
        
        /*
         * write string/object tuple
         * read using multiple wildcard
         * should return the written object
         */
        [Test]
        public void ReadWildcard3_1()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*", null, "fiel*"}));
            
            _tupleSpace.Write(Tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple3, result);
        }
        
        /*
         * write string/object tuple
         * read using multiple wildcard
         * should return the written object
         */
        [Test]
        public void ReadWildcard3_2()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*1", "DADTestB", "*"}));
            
            _tupleSpace.Write(Tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple3, result);
        }
        
        /*
         * write string tuple and string/object tuple
         * read using wildcard that matches the first field of both
         * should return only the string tuple
         */
        [Test]
        public void ReadWildcard4()
        {
            var tupleSchemaWildcard = new TupleSchema(new Tuple(new List<object> {"*1"}));
            
            _tupleSpace.Write(Tuple1);
            _tupleSpace.Write(Tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(Tuple1, result);
            Assert.IsTrue(result.Count == 1);
        }

        /*
         * take
         * should return empty list
         */
        [Test]
        public void Take1()
        {
            var result = _tupleSpace.Take(_tuple1Schema);
            Assert.IsEmpty(result);
        }

        /*
         * Write, take
         * take should return the written value
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Take2(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
            var tupleSchema = new TupleSchema(tuple);
            var result = _tupleSpace.Take(tupleSchema);
            Assert.Contains(tuple, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        /*
         * Write, take, read
         * take should return the written value
         * read should return empty
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Take3(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
            var tupleSchema = new TupleSchema(tuple);
            var result = _tupleSpace.Take(tupleSchema);
            Assert.Contains(tuple, result);
            Assert.IsTrue(result.Count == 1);
            var result2 = _tupleSpace.Read(tupleSchema);
            Assert.IsEmpty(result2);
        }
        
        /*
         * Write, write, take, read
         * take should take all
         * read should read zero
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Take4(Tuple tuple)
        {
            _tupleSpace.Write(tuple);
            _tupleSpace.Write(tuple);
            var tupleSchema = new TupleSchema(tuple);
            var result = _tupleSpace.Take(tupleSchema);
            Assert.Contains(tuple, result);
            Assert.AreEqual(tuple, result.ToArray()[0]);
            Assert.AreEqual(tuple, result.ToArray()[1]);
            var result2 = _tupleSpace.Read(tupleSchema);
            Assert.IsEmpty(result2);
        }
        
    }
}
