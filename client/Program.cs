using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            Client client = new Client();

            Tuple tuple = new Tuple(new List<object>(new object[] { "hello", "world", 2 }));
            client.Write(tuple);


            Console.ReadLine();
        }

    }
}