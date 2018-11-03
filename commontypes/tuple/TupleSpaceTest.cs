using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CommonTypes
{
    [TestFixture()]
    public class TupleSpaceTest
    {
        //TODO check how to make proper setups and teardowns
        [Test()]
        public void Write1()
        {
            TupleSpace tupleSpace = new TupleSpace();
            Tuple tuple1 = new Tuple(new List<object>() { "field1" });
            tupleSpace.Write(tuple1);
            Tuple tuple2 = tupleSpace.Read(new TupleSchema(tuple1));
            Assert.AreEqual(tuple1, tuple2);
        }

        [Test()]
        public void Read1()
        {
            TupleSpace tupleSpace = new TupleSpace();
            Tuple tuple1 = new Tuple(new List<object>() { "field1" });
            tupleSpace.Write(tuple1);
            Tuple tuple2 = tupleSpace.Read(new TupleSchema(new Tuple(new List<object>() { "*field1" })));
            Assert.AreEqual(tuple1, tuple2);
        }

        [Test()]
        public void Write3()
        {

        }

        [Test()]
        public void Write4()
        {

        }
    }
}
