using CommonTypes;
using ClientNamespace;
using ServerNamespace;

using System.Collections.Generic;

namespace PCS
{
    abstract class PCSRemotingAbstract : PCSRemotingInterface
    {
        protected readonly Dictionary<string, Client> clients;
        protected readonly Dictionary<string, Server> servers;

        public PCSRemotingAbstract()
        {
            clients = new Dictionary<string, Client>();
            servers = new Dictionary<string, Server>();
        }

        public abstract void Client(string client_id, string URL, string[] script);
        public abstract void Server(string server_id, string URL, int min_delay, int max_delay);
    }
}
