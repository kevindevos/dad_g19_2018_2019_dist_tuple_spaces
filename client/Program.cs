using System;
using System.Collections.Generic;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace
{
    public static class Program
    {
        public static void Main(string[] args) {
            ClientSMR client = new ClientSMR();

            Tuple tuple1 = new Tuple(new List<object>(new object[] { "hello", "world", 2 }));
            client.Write(tuple1);

            Tuple tuple2 = client.Read(tuple1);
            client.Log("Output: " + tuple2.ToString());

            Tuple tuple3 = client.Take(tuple1);
            client.Log("Output: " + tuple3.ToString());

            Tuple tuple4 = client.Read(tuple1);
            client.Log("Output: " + tuple4);

        }

    }
}