using System;
using CommonTypes;

namespace ServerNamespace
{
    public static class Program
    {
        /*
         * Arguments: SMR|XL [host port serverId]
         */
        public static void Main(string[] args) {
            
            if(args.Length != 1 && args.Length != 4)
                throw new ArgumentException("Valid arguments: SMR|XL [host port serverId]");

            var implementation = args[0];
            Server server;
            var remoteUrl = "";

            if (args.Length > 1)
            {
                var host = args[1];
                var port = int.Parse(args[2]);
                var serverId = args[3];
                remoteUrl = RemotingEndpoint.BuildRemoteUrl(host, port, serverId);
            }

            switch (implementation)
            {
                case "SMR":
                {
                    server = remoteUrl == "" ? new ServerSMR() : new ServerSMR(remoteUrl);
                    break;
                }
                case "XL":
                {
                    server = remoteUrl == "" ? new ServerSMR() : new ServerSMR(remoteUrl); //TODO change to ServerXL
                    break;
                }
                default:
                    throw new ArgumentException("First argument must be SMR or XL. It was: " + implementation);
            }
                
            Console.WriteLine("Server {0} running on {1} ...", implementation, server.EndpointURL);
            Console.ReadLine();
            
        }
    }
}