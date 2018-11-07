using CommonTypes.tuple;
using System.Collections.Generic;
using ClientNamespace;
using Tuple = CommonTypes.tuple.Tuple;
using System;
using System.Linq;

namespace ClientNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            Client client = new Client();
            client.ClientRequestSeqNumber = 0;
            Tuple tuple = new Tuple(new List<object>(new object[] { "hello", "world", 2 }));
            client.Write(tuple);

            Console.WriteLine("Client Started, press <enter> to leave.");
            Console.ReadLine();
        }

    }
}