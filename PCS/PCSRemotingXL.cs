using ServerNamespace;

namespace PCS
{
    class PCSRemotingXL : PCSRemotingAbstract
    {
        override public void Server(string server_id, string URL, int min_delay, int max_delay)
        {
            Server server = new ServerXL(URL, serverPortCounter);
            servers.Add(server_id, server);
            serverPortCounter++;
        }
    }
}
