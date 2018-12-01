using System.Collections.Generic;
using ClientNamespace;
using CommonTypes;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;
using ServerNamespace;
using ServerNamespace.XL;

namespace Tests
{
    [TestFixture(1)]
    [TestFixture(2)]
    //[TestFixture(3)]
    //[TestFixture(4)]
    //[TestFixture(5)]
    [Ignore("Ignore XL test for now")]
    public class ServerXLTest : ServerTest
    {
        public ServerXLTest(int nServers) : base(nServers)
        {
        }

        [SetUp]
        public void Init()
        {
            Client1 = new ClientXL("tcp://localhost:9090/c1");
            Client2 = new ClientXL("tcp://localhost:9091/c2");
        }

        [TearDown]
        public void Dispose()
        {            
            Client1.DisposeChannel();
            Client2.DisposeChannel();
        }
    }
}