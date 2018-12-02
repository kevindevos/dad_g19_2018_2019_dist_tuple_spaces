using System.Collections;
using System.Collections.Generic;
using ClientNamespace;
using CommonTypes;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;
using ServerNamespace;

namespace Tests
{
    [TestFixture, NonParallelizable]
    public abstract class ServerTest
    {
        private readonly int _nServers;

        protected List<Server> _serverList;
        
        protected Client Client1;
        protected Client Client2;
        
        protected static readonly Tuple Tuple1 = new Tuple(new List<object> { "field1" });
        protected static readonly Tuple Tuple2 = new Tuple(new List<object> { new DADTestA(1, "2") });
        protected static readonly Tuple Tuple3 = new Tuple(new List<object> { "field1", new DADTestB(1, "field2", 3), "field3" });
        protected TupleSchema Tuple1Schema;
        protected TupleSchema Tuple2Schema;
        protected TupleSchema Tuple3Schema;

        protected ServerTest(int nServers)
        {
            this._nServers = nServers;
        }


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverList = new List<Server>();
                        
            Tuple1Schema = new TupleSchema(Tuple1);
            Tuple2Schema = new TupleSchema(Tuple2);
            Tuple3Schema = new TupleSchema(Tuple3);
        }
        
        [SetUp]
        public void SetUp()
        {
            for (var i = 1; i <= _nServers; i++)
            {
                _serverList.Add(new ServerSMR(RemotingEndpoint.BuildRemoteUrl("localhost", 8080+i, "s"+i)));    
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            foreach (var server in _serverList)
            {
                server.DisposeChannel();
            }
            _serverList.Clear();
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
         * test a simple write 
         */
        [Test, NonParallelizable, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Write(Tuple tuple)
        {
            Client1.Write(tuple);
        }
        
        /*
         * test a write and a read
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples)), Timeout(5000)]
        public void WriteRead(Tuple tuple)
        {
            Client1.Write(tuple);
            var result = Client1.Read(tuple);
            Assert.AreEqual(tuple, result);
        }
        
        /*
         * test a write and a read
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples)), Timeout(5000)]
        public void WriteTakeRead(Tuple tuple)
        {
            Client1.Write(tuple);
            Client1.Write(tuple);
            var result = Client1.Take(tuple);
            Assert.AreEqual(tuple, result);
            result = Client1.Read(tuple);
            Assert.AreEqual(tuple, result);
        }
    }
}