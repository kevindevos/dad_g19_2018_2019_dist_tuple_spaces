using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Crash(string processname)
        {
            if (!servers.ContainsKey(processname)) return;
            servers[processname].Crash();
            servers.Remove(processname);
        }
        
        public string Status()
        {
            string response = "";
            Server server;

            foreach (var processname in servers.Keys.ToArray())
            {
                server = servers[processname];
            
                response += "\n===============\n";
                response += $"processname: {processname}\n";
                response += server.Status();   
            }

            Console.WriteLine("Response...");
            Console.WriteLine(response);
            return response;
        }
        
        private void Log(string text) {
            Console.WriteLine("[PCS]: " + text);
        }
    }
}
