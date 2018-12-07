using System;
using CommonTypes;

namespace ClientNamespace
{
    public static class Program
    {
        /*
         * Arguments: SMR|XL [host port clientId]
         */
        public static void Main(string[] args)
        {
            if(args.Length != 1 && args.Length != 4)
                throw new ArgumentException("Valid arguments: SMR|XL [host port clientId]");

            var implementation = args[0];
            Client client;
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
                    client = remoteUrl == "" ? new ClientSMR() : new ClientSMR(remoteUrl);
                    break;
                }
                case "XL":
                {
                    client = remoteUrl == "" ? new ClientXL() : new ClientXL(remoteUrl);
                    break;
                }
                default:
                    throw new ArgumentException("First argument must be SMR or XL. It was: " + implementation);
            }
                
            Console.WriteLine("Client {0} running on {1} ...", implementation, client.EndpointURL);
            client.runScript();
            
            Console.ReadLine();
            
            //TODO interactive input or read script as in PuppetMaster
        }

    }
}