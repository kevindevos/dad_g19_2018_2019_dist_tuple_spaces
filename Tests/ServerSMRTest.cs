using System.Collections.Generic;
using ClientNamespace;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;
using ServerNamespace;

namespace Tests
{
    [TestFixture]
    public class ServerSMRTest
    {
        private Server _server1;
        private Server _server2;
        private Client _client1;
        
        private Tuple _tuple1;
        private Tuple _tuple2;
        private Tuple _tuple3;
        private TupleSchema _tuple1Schema;
        private TupleSchema _tuple2Schema;
        private TupleSchema _tuple3Schema;
        
        [SetUp]
        public void Init()
        {
            _server1 = new ServerSMR("tcp://localhost:8081/s1"); 
            //_server2 = new ServerSMR("tcp://localhost:8082/s2"); 
            _client1 = new ClientSMR("tcp://localhost:9090/c1");
            
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
            _server1.DisposeChannel();
            //_server2.DisposeChannel();
            _client1.DisposeChannel();
        }
        
        /*
         * test 
         */
        [Test]
        public void Test1()
        {
            _client1.Write(_tuple1);
        }
        
        /*
         * test 
         */
        [Test]
        public void Test2()
        {
            _client1.Write(_tuple1);
            var result = _client1.Read(_tuple1);
            Assert.AreEqual(_tuple1, result);
        }
    }
}