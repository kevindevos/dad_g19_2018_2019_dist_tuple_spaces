using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PCS
{
    class Program
    {
        static readonly int PORT = 10000;

        static int Mode()
        {
            int value = -1;
            string response;

            Console.WriteLine("===== Welcome to PCS =====");
            Console.WriteLine("Choose a tuple space implementation:");
            Console.WriteLine("1. SMR");
            Console.WriteLine("2. XML");

            while (value != 1 && value != 2)
            {
                Console.Write("\n[1|2] ");
                response = Console.ReadLine();
                int.TryParse(response, out value);
            }

            return value;

        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, false);

            int mode = Mode();

            if (mode == 1)
            {
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(PCSRemotingSMR),
                    "PCS",
                    WellKnownObjectMode.Singleton
                );

                Console.WriteLine("Running SMR mode on port {0}", PORT);
            }

            else
            {
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(PCSRemotingXL),
                    "PCS",
                    WellKnownObjectMode.Singleton
                );

                Console.WriteLine("Running XL mode on port {0}", PORT);
            }

            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
