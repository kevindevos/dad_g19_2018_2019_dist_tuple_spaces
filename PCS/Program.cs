using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PCS
{
    class Program
    {
        static readonly int PORT = 10000;

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PCSRemotingXL),
                "PCS",
                WellKnownObjectMode.Singleton
            );

            Console.WriteLine("Running on port {0}", PORT);
            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
