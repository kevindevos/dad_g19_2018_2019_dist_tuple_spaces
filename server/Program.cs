using System;

namespace ServerNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            Server server1 = new ServerSMR(8080);
            ((ServerSMR)server1).UpgradeToMaster();
            
            Server server2 = new ServerSMR(8081);
            Server server3 = new ServerSMR(8082);

            Console.WriteLine("Server Started, press <enter> to leave.");
            string hello = "Hello";
            server1.Log(hello);
            /*server2.Log(hello);
            server3.Log(hello);*/
            Console.ReadLine();
        }
    }
}