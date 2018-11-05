using System;
using ServerNamespace.Server;

namespace ServerNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            Server.Server server = new ServerSmr();
            
            // (main thread loop)
            // TODO if master, loop through request list 
            // verify if request is ready to be executed ( there is no previous request with lower sequence number from same client to be executed)  or other cases?
            // broadcast the request to other servers as an order 
            // perform the request as the master 

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }
    }
}