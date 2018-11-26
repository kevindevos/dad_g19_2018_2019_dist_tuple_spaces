using System.Collections.Generic;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;

namespace Tests {
    
    [TestFixture]
    public class TupleSpaceTest
    {
        private TupleSpace _tupleSpace;
        private Tuple _tuple1;
        private Tuple _tuple2;
        private Tuple _tuple3;
        private TupleSchema _tuple1Schema;
        private TupleSchema _tuple2Schema;
        private TupleSchema _tuple3Schema;
        
        [SetUp]
        public void Init()
        {
            _tupleSpace = new TupleSpace();
            _tuple1 = new Tuple(new List<object> { "field1" });
            _tuple2 = new Tuple(new List<object> { new DADTestA(1, "2") });
            _tuple3 = new Tuple(new List<object> { "field1", new DADTestB(1, "field2", 3), "field3" });
            _tuple1Schema = new TupleSchema(_tuple1);
            _tuple2Schema = new TupleSchema(_tuple2);
            _tuple3Schema = new TupleSchema(_tuple3);
            
        }

        [TearDown]
        public void Dispose()
        {
            
        }
       
        /*
         * test the write of string
         */
        [Test]
        public void Write1()
        {
            _tupleSpace.Write(_tuple1);
        }
        
        /*
         * test the write of an object
         */
        [Test]
        public void Write2()
        {
            _tupleSpace.Write(_tuple2);
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
         * test the read of a string using a perfect match schema
         */
        [Test]
        public void Read1()
        {
            _tupleSpace.Write(_tuple1);
            var result = _tupleSpace.Read(_tuple1Schema);
            Assert.Contains(_tuple1, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        /*
         * test the read of an object using a perfect match schema
         */
        [Test]
        public void Read2()
        {
            _tupleSpace.Write(_tuple2);
            var result = _tupleSpace.Read(_tuple2Schema);
            Assert.Contains(_tuple2, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        /*
         * test the read of an string/object tuple using a perfect match schema
         */
        [Test]
        public void Read3()
        {
            _tupleSpace.Write(_tuple3);
            var result = _tupleSpace.Read(_tuple3Schema);
            Assert.Contains(_tuple3, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        /*
         * write, read
         * should return the written value
         */
        private void ReadWildcard1_0(TupleSchema tupleSchemaWildcard)
        {
            _tupleSpace.Write(_tuple1);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple1, result);
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
            
            _tupleSpace.Write(_tuple2);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple2, result);
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
            
            _tupleSpace.Write(_tuple2);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple2, result);
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
            
            _tupleSpace.Write(_tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple3, result);
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
            
            _tupleSpace.Write(_tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple3, result);
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
            
            _tupleSpace.Write(_tuple1);
            _tupleSpace.Write(_tuple3);
            var result = _tupleSpace.Read(tupleSchemaWildcard);
            Assert.Contains(_tuple1, result);
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
        [Test]
        public void Take2()
        {
            _tupleSpace.Write(_tuple1);
            var result = _tupleSpace.Take(_tuple1Schema);
            Assert.Contains(_tuple1, result);
            Assert.IsTrue(result.Count == 1);
        }
        
        /*
         * Write, take, read
         * take should return the written value
         * read should return empty
         */
        [Test]
        public void Take3()
        {
            _tupleSpace.Write(_tuple1);
            var result = _tupleSpace.Take(_tuple1Schema);
            Assert.Contains(_tuple1, result);
            Assert.IsTrue(result.Count == 1);
            var result2 = _tupleSpace.Read(_tuple1Schema);
            Assert.IsEmpty(result2);
        }
        
        /*
         * Write, write, take, read
         * take should only take 1
         * read should read 1
         */
        [Test]
        public void Take4()
        {
            _tupleSpace.Write(_tuple1);
            _tupleSpace.Write(_tuple1);
            var result = _tupleSpace.Take(_tuple1Schema);
            Assert.Contains(_tuple1, result);
            Assert.IsTrue(result.Count == 1);
            var result2 = _tupleSpace.Read(_tuple1Schema);
            Assert.IsTrue(result2.Count == 1);
        }
        
    }
}
