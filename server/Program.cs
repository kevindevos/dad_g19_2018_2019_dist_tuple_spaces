using System;

namespace ServerNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            // TESTING //

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

            // END TESTING //

            Console.ReadLine();
        }
    }
}