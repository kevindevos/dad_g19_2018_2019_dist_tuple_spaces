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

        public static void testSMR() {
            ServerSMR server1 = new ServerSMR(8080);
            ServerSMR server2 = new ServerSMR(8081);
            ServerSMR server3 = new ServerSMR(8082);

            string hello = "Hello";
            server3.UpgradeToMaster();
            server1.MasterEndpointURL = server3.EndpointURL;
            server2.MasterEndpointURL = server3.EndpointURL;
            //server3.MasterEndpoint = server3;

            server1.Log(hello);
            server2.Log(hello);
            server3.Log(hello);

            Console.ReadLine();
        }

        public static void testXL() {

        }
    }
}