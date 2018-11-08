using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.message;
using System.Collections;

namespace CommonTypes {
    public delegate Message RemoteAsyncDelegate(Message message);

    public abstract class RemotingEndpoint : MarshalByRefObject  {
        public const int NUM_SERVERS = 3;

        protected const string defaultServerHost = "localhost";
        protected const int defaultServerPort = 8080;

        protected const string defaultClientHost = "localhost";
        protected const int defaultClientPort = 8070;

        private string _objIdentifier;
        public string ObjIdentifier { get; private set; }

        private List<RemotingEndpoint> _knownServerRemotes;
        public List<RemotingEndpoint> KnownServerRemotes;

        private string _endpointURL;
        public string EndpointURL { get; private set; }

        private TcpChannel _tcpChannel;
        public TcpChannel TcpChannel { get; private set; }

        private int _port;
        public int Port { get; set; }

        private string _host;
        public string Host { get; private set; }


        public RemotingEndpoint(string host, int port, string objIdentifier) {
            Host = host;
            Port = port;
            ObjIdentifier = objIdentifier;

            EndpointURL = BuildRemoteUrl(host, port, objIdentifier+port);

            // register tcp channel and service
            IDictionary dictionary = new System.Collections.Hashtable();
            dictionary["name"] = "tcp" + port;
            dictionary["port"] = port;
            TcpChannel = new TcpChannel(dictionary, null,null);
            ChannelServices.RegisterChannel(TcpChannel, false);
            RemotingServices.Marshal(this, objIdentifier + port, typeof(RemotingEndpoint));

            KnownServerRemotes = GetKnownServerRemotes();
        }

        protected RemotingEndpoint(string objIdentifier) {
            ObjIdentifier = objIdentifier;
        }

        private List<RemotingEndpoint> GetKnownServerRemotes() {
            List<RemotingEndpoint> knownRemotes = new List<RemotingEndpoint>();

            for(int i = defaultServerPort; i < defaultServerPort+NUM_SERVERS; i++) {
                if (i == Port) continue;
                string serverUrl = (BuildRemoteUrl(defaultServerHost, i, "Server"+i));

                knownRemotes.Add(GetRemoteEndpoint(serverUrl));
            }

            return knownRemotes;
        }

        public static RemotingEndpoint GetRemoteEndpoint(string host, int destPort, string objIdentifier) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                BuildRemoteUrl(host, destPort, objIdentifier+destPort));

            return remote;
        }

        public static RemotingEndpoint GetRemoteEndpoint(string url) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint),
                url);

            return remote;
        }

        public Message SendMessageToRemote(RemotingEndpoint remotingEndpoint, Message message) {
            try {
                RemoteAsyncDelegate remoteDel = new RemoteAsyncDelegate(remotingEndpoint.OnReceiveMessage);
                IAsyncResult ar = remoteDel.BeginInvoke(message, null, null);
                ar.AsyncWaitHandle.WaitOne();
                return remoteDel.EndInvoke(ar);
            }
            catch(Exception e) {
                Console.WriteLine("Server at " + remotingEndpoint.EndpointURL + " is unreachable. (For more detail: " + e.Message + ")");
                throw new NotImplementedException();
            }
        }


        public Message SendMessageToRemoteURL(string remoteURL, Message message) {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remoteURL);
            return SendMessageToRemote(remotingEndpoint, message);
        }

        public List<Message> SendMessageToKnownServers(Message message) {
            List<Message> messages = new List<Message>();

            for (int i = 0; i < KnownServerRemotes.Count; i++) {
                messages.Add(SendMessageToRemote(KnownServerRemotes.ElementAt(i), message));
            }

            return messages;
        }

        public static string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        public abstract Message OnReceiveMessage(Message message);

        public abstract Message OnSendMessage(Message message);

        public string GetRemoteEndpointURL() {
            return EndpointURL;
        }
    }
}
