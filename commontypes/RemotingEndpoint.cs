using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.message;

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
            endpointURL = BuildRemoteUrl(host, port, objIdentifier);

            // register tcp channel and service
            tcpChannel = new TcpChannel(port);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingServices.Marshal(this, objIdentifier, typeof(RemotingEndpoint));

            knownServerRemotes = GetKnownServerRemotes();
        }

        protected RemotingEndpoint(string objIdentifier) {
            this.objIdentifier = objIdentifier;
        }

        private List<RemotingEndpoint> GetKnownServerRemotes() {
            List<RemotingEndpoint> knownRemotes = new List<RemotingEndpoint>();

            for(int i = defaultServerPort; i < defaultServerPort+3; i++) {
                if (i == port) continue;
                string serverUrl = (BuildRemoteUrl(defaultServerHost, i, "Server"));
                knownRemotes.Add(GetRemoteEndpoint(serverUrl));
            }

            return knownRemotes;
        }

        public RemotingEndpoint GetRemoteEndpoint(string host, int destPort, string objIdentifier) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                BuildRemoteUrl(host, destPort, objIdentifier));

            return remote;
        }

        public RemotingEndpoint GetRemoteEndpoint(string url) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                url);

            return remote;
        }

        public void SendMessateToRemote(RemotingEndpoint remotingEndpoint, Message message) {
            RemoteAsyncDelegate remoteDel = remotingEndpoint.OnReceiveMessage;
            remoteDel.BeginInvoke(message, null, null);
        }

        public void SendMessateToRemoteURL(string remoteURL, Message message) {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remoteURL);
            SendMessateToRemote(remotingEndpoint, message);
        }

        public void SendMessageToKnownServers(Message message) {
            for (int i = 0; i < knownServerRemotes.Count; i++) {
                SendMessateToRemote(knownServerRemotes.ElementAt(i), message);
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
