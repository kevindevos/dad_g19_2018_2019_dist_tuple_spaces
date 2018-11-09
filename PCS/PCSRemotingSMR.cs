using ClientNamespace;
using ServerNamespace;

namespace PCS
{
    class PCSRemotingSMR : PCSRemotingAbstract
    {
        public override void Client(string client_id, string URL, string[] script)
        {
            // TODO add new constructor to accept URL instead of host and port
            // TODO add script functionality

            // Client client = new ClientSMR(URL);
            // clients.Add(client_id, client);
        }

        override public void Server(string server_id, string URL, int min_delay, int max_delay)
        {
            // TODO add new constructor to accept URL instead of host and port

            // Server server = new ServerSMR(URL);
            // servers.Add(server_id, server);
        }
    }
}
