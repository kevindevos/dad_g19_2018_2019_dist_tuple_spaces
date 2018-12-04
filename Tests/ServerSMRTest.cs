using System;
using System.Collections.Generic;
using ClientNamespace;
using CommonTypes;
using CommonTypes.tuple;
using CommonTypes.tuple.tupleobjects;
using NUnit.Framework;
using ServerNamespace;

namespace Tests
{
    [TestFixture(1)]
    [TestFixture(2)]
    [TestFixture(3)]
    [TestFixture(4)]
    [TestFixture(5)]
    public class ServerSMRTest : ServerTest
    {
        public ServerSMRTest(int nServers) : base(nServers)
        {
            
        }


        [SetUp]
        public void Init()
        {
            for (var i = 1; i <= _nServers; i++)
            {
                _serverList.Add(new ServerSMR(RemotingEndpoint.BuildRemoteUrl("localhost", 8080+i, "s"+i)));    
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
    }
}