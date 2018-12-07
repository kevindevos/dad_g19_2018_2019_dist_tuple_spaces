using ClientNamespace;
using CommonTypes;
using NUnit.Framework;
using ServerNamespace;

namespace Tests
{
    [TestFixture(1)]
    [TestFixture(2)]
/*    [TestFixture(3)]
    [TestFixture(4)]
    [TestFixture(5)]*/
    public class ServerSMRTest : ServerTest
    {
        public ServerSMRTest(int nServers) : base(nServers) {}

        [SetUp]
        public void Init()
        {
            for (var i = 1; i <= _nServers; i++)
            {
                var url = RemotingEndpoint.BuildRemoteUrl("localhost", 8080 + i, "s" + i);
                var smr = new ServerSMR(url);
                _serverList.Add(smr);    
            }
            
            Client1 = new ClientSMR("tcp://localhost:8010/c1");
            Client2 = new ClientSMR("tcp://localhost:8011/c2");
        }

        [TearDown]
        public void Dispose()
        {
           Client1.DisposeChannel();
           Client2.DisposeChannel();
        }
        
        // Two test
        // 1. Kill the master
        // 2. Kill the non-master
        [Test, Ignore(""), Timeout(5000)]
        public void FailureTest1([Values(1)] int serverIndex)
        {
            if (_nServers < 2)
            {
                Assert.Ignore("Only test for >=2 servers.");
                return;
            }

            _serverList[serverIndex].DisposeChannel();
            Client1.Write(Tuple1);
            var result = Client1.Read(Tuple1);
            Assert.AreEqual(Tuple1, result);
        }
        
        [Test, Ignore(""), Timeout(5000)]
        public void FailureTest2([Values(0)] int serverIndex)
        {
            if (_nServers < 2)
            {
                Assert.Ignore("Only test for >=2 servers.");
                return;
            }

            _serverList[serverIndex].DisposeChannel();
            _serverList.RemoveAt(serverIndex);
            //_serverList[0].SetView(new View(_serverList.ConvertAll(input => input.EndpointURL), 1));
            Client1.Write(Tuple1);
            var result = Client1.Read(Tuple1);
            Assert.AreEqual(Tuple1, result);
        }
    }
}