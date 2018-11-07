using ServerNamespace;

namespace PCS
{
    class PCSRemotingSMR : PCSRemotingAbstract
    {
        override public void Server(string server_id, string URL, int min_delay, int max_delay)
        {
            Server server = new ServerSMR(URL, serverPortCounter);
            servers.Add(server_id, server);
            serverPortCounter++;
        }
    }
}
