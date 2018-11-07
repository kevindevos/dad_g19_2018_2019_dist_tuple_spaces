using CommonTypes;
using ClientNamespace;
using ServerNamespace;

using System.Collections.Generic;

namespace PCS
{
    abstract class PCSRemotingAbstract : PCSRemotingInterface
    {
        protected int clientPortCounter;
        protected int serverPortCounter;

        protected readonly Dictionary<string, Client> clients;
        protected readonly Dictionary<string, Server> servers;

        public PCSRemotingAbstract()
        {
            clientPortCounter = 20000;
            serverPortCounter = 40000;

            clients = new Dictionary<string, Client>();
            servers = new Dictionary<string, Server>();
        }

        public void Client(string client_id, string URL, string[] script)
        {
            Client client = new Client(URL, clientPortCounter);
            clients.Add(client_id, client);
            clientPortCounter++;
        }

        public void Status()
        {
            // TODO implement the Status method on the clients and servers
        }

        public abstract void Server(string server_id, string URL, int min_delay, int max_delay);
    }
}
