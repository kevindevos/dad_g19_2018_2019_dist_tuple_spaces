using System.Collections;
using System.Collections.Generic;

namespace CommonTypes
{
    public interface PCSRemotingInterface
    {
        void Server(string server_id, string URL, int min_delay, int max_delay, IEnumerable<string> serverUrls);
        void Client(string client_id, string URL, string[] script, IEnumerable<string> serverUrls);
    }
}
