using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.server;

namespace CommonTypes {
    public abstract class RemotingEndpoint : MarshalByRefObject, IRemoting  {
        protected const string defaultServerHost = "localhost";
        protected const int defaultServerPort = 8080;

        protected const string defaultClientHost = "localhost";
        protected const int defaultClientPort = 8070;
        protected string objIdentifier;
        protected List<IRemoting> knownServerRemotes;
        protected string endpointURL;

        protected TcpChannel tcpChannel;

        protected int port;
        protected string host;

        public RemotingEndpoint(string host, int port, string objIdentifier) {
            this.host = host;
            this.port = port;
            this.objIdentifier = objIdentifier;
            this.endpointURL = BuildRemoteUrl(host, port, objIdentifier);
            this.knownServerRemotes = GetKnownServerRemotes();

            RegisterTcpChannel();
            RegisterService();
        }

        protected RemotingEndpoint(string objIdentifier) {
            this.objIdentifier = objIdentifier;
        }

        private List<IRemoting> GetKnownServerRemotes() {
            List<IRemoting> knownRemotes = new List<IRemoting>();

            for(int i = defaultServerPort; i < defaultServerPort+3; i++) {
                if (i == this.port) continue;
                string serverUrl = (BuildRemoteUrl(defaultServerHost, i, "Server"));
                knownRemotes.Add(GetRemote(serverUrl));
            }

            return knownRemotes;
        }

        public IRemoting GetRemote(string host, int destPort, string objIdentifier) {
            IRemoting remote = (IRemoting)Activator.GetObject(
                typeof(IRemoting),
                BuildRemoteUrl(host, destPort, objIdentifier));

            return remote;
        }

        public IRemoting GetRemote(string url) {
            IRemoting remote = (IRemoting)Activator.GetObject(
                typeof(IRemoting),
                url);

            return remote;
        }

        public string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        public void OnReceiveMessage(Message message) { }

        public void OnSendMessage(Message message) { }


        public void RegisterService() {
            RemotingServices.Marshal(this, objIdentifier, typeof(RemotingEndpoint));
        }

        public void RegisterTcpChannel() {
            tcpChannel = new TcpChannel(port);
            ChannelServices.RegisterChannel(tcpChannel, false);
        }
    }
}
