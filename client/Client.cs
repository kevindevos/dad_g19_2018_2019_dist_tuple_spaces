using CommonTypes;
using CommonTypes.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ClientNamespace {
    class Client : MarshalByRefObject, IRemoting, ITupleOperations {
        private const string defaultServerHost = "localhost";
        private int serverPort;

        public const int defaultClientPort = 8080;
        public const string defaultClientHost = "localhost";

        // TODO sequence number thread safe??
        private int clientRequestSeqNumber { get { return clientRequestSeqNumber; } set { clientRequestSeqNumber = value; } }

        // works as a client identifier for the servers and allows them to know where to send a message
        public string clientRemoteURL;

        // remote object for sending messages to the server
        private IRemoting remote;

        // this could be later changed for letting client know of other servers, initialized to 3 values as default
        private List<int> knownServerPorts = new List<int>(new int[] { 8086, 8087, 8088 });

        private TcpChannel tcpChannel;

        static void Main(string[] args) {
            Client client = new Client();
            client.clientRequestSeqNumber = 0;

            client.RegisterTcpChannel();
            client.RegisterService();

            // TODO call connect, with specific port, or default port?
        }

        public Client() {
            clientRemoteURL = BuildRemoteUrl(defaultClientHost, defaultClientPort, "Client");
        }

        public Client(string host, int port) {
            clientRemoteURL = BuildRemoteUrl(host, port, "Client");
        }

        public IRemoting Connect(string url, string objIdentifier) {
            for(int i = 0; i < knownServerPorts.Capacity; i++) {
                try {
                    IRemoting remote = (IRemoting)Activator.GetObject(
                        typeof(IRemoting),
                        BuildRemoteUrl(url, knownServerPorts[i], objIdentifier));
                    return remote;
                }
                catch {
                    // TODO
                }
            }

            return null;
        }

        public IRemoting Connect(string url, int destPort, string objIdentifier) {
            IRemoting remote = (IRemoting)Activator.GetObject(
                typeof(IRemoting),
                BuildRemoteUrl(url, destPort, objIdentifier));

            return remote;
        }

        // parse a message from a server
        public void OnReceiveMessage(Message message) {
            throw new NotImplementedException();
        }

        public void RegisterTcpChannel() {
            tcpChannel = new TcpChannel(serverPort);
            ChannelServices.RegisterChannel(tcpChannel, true);
        }

        public void RegisterService() {
            RemotingServices.Marshal(this, "Client", typeof(Client));
        }

        public string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        public void Write(CommonTypes.Tuple tuple) {
            Request request = new Request(clientRequestSeqNumber, clientRemoteURL, RequestType.WRITE, tuple);
            remote.OnReceiveMessage(request);
        }

        public CommonTypes.Tuple Read(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }

        public CommonTypes.Tuple Take(TupleSchema tupleSchema) {
            throw new NotImplementedException();
        }
    }
}
