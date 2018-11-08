using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.message;
using System.Collections;

namespace CommonTypes {
    public delegate void RemoteAsyncDelegate(Message message);

    public abstract class RemotingEndpoint : MarshalByRefObject  {
        protected const string defaultServerHost = "localhost";
        protected const int defaultServerPort = 8080;

        protected const string defaultClientHost = "localhost";
        protected const int defaultClientPort = 8070;
        protected string objIdentifier;
        public List<RemotingEndpoint> knownServerRemotes;
        public string endpointURL;

        protected TcpChannel tcpChannel;

        protected int port;
        protected string host;

        public RemotingEndpoint(string host, int port, string objIdentifier) {
            this.host = host;
            this.port = port;
            this.objIdentifier = objIdentifier;

            endpointURL = BuildRemoteUrl(host, port, objIdentifier+port);

            // register tcp channel and service
            IDictionary dictionary = new System.Collections.Hashtable();
            dictionary["name"] = "tcp" + port;
            dictionary["port"] = port;
            tcpChannel = new TcpChannel(dictionary, null,null);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingServices.Marshal(this, objIdentifier+port, typeof(RemotingEndpoint));

            knownServerRemotes = GetKnownServerRemotes();
        }

        protected RemotingEndpoint(string objIdentifier) {
            this.objIdentifier = objIdentifier;
        }

        private List<RemotingEndpoint> GetKnownServerRemotes() {
            List<RemotingEndpoint> knownRemotes = new List<RemotingEndpoint>();

            for(int i = defaultServerPort; i < defaultServerPort+3; i++) {
                if (i == port) continue;
                string serverUrl = (BuildRemoteUrl(defaultServerHost, i, "Server"+i));

                knownRemotes.Add(GetRemoteEndpoint(serverUrl));
            }

            return knownRemotes;
        }

        public RemotingEndpoint GetRemoteEndpoint(string host, int destPort, string objIdentifier) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                BuildRemoteUrl(host, destPort, objIdentifier+destPort));

            return remote;
        }

        public RemotingEndpoint GetRemoteEndpoint(string url) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                url);

            return remote;
        }

        public void SendMessageToRemote(RemotingEndpoint remotingEndpoint, Message message) {
            try {
                RemoteAsyncDelegate remoteDel = new RemoteAsyncDelegate(remotingEndpoint.OnReceiveMessage);
                IAsyncResult ar = remoteDel.BeginInvoke(message, null, null);
                remoteDel.EndInvoke(ar);
            }
            catch(Exception e) {
                Console.WriteLine("Server at " + remotingEndpoint.endpointURL + " is unreachable. (For more detail: " + e.Message + ")");
            }
        }

        public void SendMessateToRemoteURL(string remoteURL, Message message) {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remoteURL);
            SendMessageToRemote(remotingEndpoint, message);
        }

        public void SendMessageToKnownServers(Message message) {
            for (int i = 0; i < knownServerRemotes.Count; i++) {
                SendMessageToRemote(knownServerRemotes.ElementAt(i), message);
            }
        }

        public string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        public abstract void OnReceiveMessage(Message message);

        public abstract void OnSendMessage(Message message);

        public string GetRemoteEndpointURL() {
            return endpointURL;
        }
    }
}
