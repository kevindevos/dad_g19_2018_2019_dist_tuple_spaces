using System;
using System.Collections.Generic;
using ClientNamespace;
using CommonTypes;
using ServerNamespace;

namespace PCS
{
    public abstract class PCSRemotingAbstract : MarshalByRefObject, PCSRemotingInterface
    {
        protected readonly Dictionary<string, Client> clients;
        protected readonly Dictionary<string, Server> servers;

        public PCSRemotingAbstract()
        {
            clients = new Dictionary<string, Client>();
            servers = new Dictionary<string, Server>();
        }

        public abstract void Client(string client_id, string URL, string[] script, IEnumerable<string> serverUrls);
        public abstract void Server(string server_id, string URL, int min_delay, int max_delay, IEnumerable<string> serverUrls);

    }
}
