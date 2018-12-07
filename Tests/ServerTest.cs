using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ClientNamespace;
using CommonTypes.message;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;
using ServerNamespace;
using Tuple = CommonTypes.tuple.Tuple;

namespace Tests
{
    [TestFixture]
    public abstract class ServerTest
    {
        protected readonly int _nServers;

        public delegate Tuple ClientTestReadDelegate(CommonTypes.tuple.Tuple tuple);
        protected List<Server> _serverList;
        
        protected Client Client1;
        protected Client Client2;
        
        protected static readonly Tuple Tuple1 = new Tuple(new List<object> { "field1" });
        protected static readonly Tuple Tuple2 = new Tuple(new List<object> { new DADTestA(1, "2") });
        protected static readonly Tuple Tuple3 = new Tuple(new List<object> { "field1", new DADTestB(1, "field2", 3), "field3" });
        protected TupleSchema Tuple1Schema;
        protected TupleSchema Tuple2Schema;
        protected TupleSchema Tuple3Schema;

        protected object lockObject = new object();
        protected Tuple sharedTuple = null;

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
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void Write(Tuple tuple)
        {
            Client1.Write(tuple);
        }
        
        /*
         * test a write and a read
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void WriteRead(Tuple tuple)
        {
            Client1.Write(tuple);
            var result = Client1.Read(tuple);
            Assert.AreEqual(tuple, result);
        }
        
        /*
         * test a write and a read
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void WriteTakeRead(Tuple tuple)
        {
            Client1.Write(tuple);
            Client1.Write(tuple);
            var result = Client1.Take(tuple);
            Assert.AreEqual(tuple, result);
            result = Client1.Read(tuple);
            Assert.AreEqual(tuple, result);
        }
        
        /*
         * test a read of something tha t doesnt exist yet, then write, and read should finish once written in the server
         */
        [Test, TestCaseSource(typeof(TupleDataClass), nameof(TupleDataClass.Tuples))]
        public void ReadWrite(Tuple tuple){
            // start a client to read a tuple that doesnt exist yet , and it will block until it gets something
            Thread readThread = new Thread(() => readAndSetVar(tuple));
            readThread.Start();
           
            Client2.Write(tuple);
            lock (lockObject){
                Monitor.Wait(lockObject, new TimeSpan(0, 0, 2));
                Assert.AreEqual(tuple, sharedTuple);
            }

        }

        /**
         * Helper method to run with another thread, for example for ReadWrite test, we want a client to start reading before
         * there is any tuple in the space, and then write with another client, to test if the reading client gets the result
         */
        private void readAndSetVar(Tuple tuple){
            Tuple temp = Client1.Read(tuple);

            lock (lockObject){
                sharedTuple = temp;
                Monitor.Pulse(lockObject);
            }
        }
        
      
    }
}