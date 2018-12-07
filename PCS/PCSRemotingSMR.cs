using System;
using System.Collections.Generic;
using ClientNamespace;
using ServerNamespace;

namespace PCS
{
    class PCSRemotingSMR : PCSRemotingAbstract
    {
        public override void Client(string client_id, string URL, string[] script)
        {

            var serverUrls = new List<string>();
            foreach (var server in servers.Values)
            {
                serverUrls.Add(server.EndpointURL);
                
            }
            Client client = new ClientSMR(URL, serverUrls);
            clients.Add(client_id, client);
            
            Console.WriteLine("Added client {0} [{1}] with script {2}", client_id, URL, script);
            client.runScript(script);
        }

        public override void Server(string server_id, string URL, int min_delay, int max_delay)
        {
            Server server = new ServerSMR(URL);
            servers.Add(server_id, server);
            
            Console.WriteLine("Added server {0} [{1}] with a delay between [{2},{3}]", server_id, URL, min_delay,
                max_delay);
        }
    }
}