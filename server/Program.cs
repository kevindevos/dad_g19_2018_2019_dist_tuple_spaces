using System;
using ServerNamespace.Server;

namespace ServerNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            Server.Server server = new ServerSMR();
            
            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }
    }
}