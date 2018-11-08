using CommonTypes;

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;

namespace PuppetMaster
{

    class Controller
    {
        private readonly Dictionary<string, PCSRemotingInterface> pcs;
        private readonly Dictionary<string, TcpChannel> channels;

        public Controller(string[] addrs)
        {
            pcs = new Dictionary<string, PCSRemotingInterface>();
            channels = new Dictionary<string, TcpChannel>();

            foreach (string addr in addrs)
            {
                string URL = "tcp://{0}/PCS";

                TcpChannel channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel, false);

                PCSRemotingInterface obj = (PCSRemotingInterface)Activator.GetObject(
                    typeof(PCSRemotingInterface),
                    string.Format(URL, addr)
                );

                pcs.Add(URL, obj);
                channels.Add(URL, channel);
            }
        }
    }
}
