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
        private const int NumServers = 3;

        protected const string DefaultServerHost = "localhost";
        protected const int DefaultServerPort = 8080;

        protected const string DefaultClientHost = "localhost";
        protected const int DefaultClientPort = 8070;

        private string ObjIdentifier { get; }

        protected readonly List<RemotingEndpoint> KnownServerRemotes;

        public string EndpointURL { get; }

        private TcpChannel TcpChannel { get; }

        protected int Port { private get; set; }

        private string Host { get; }


        protected RemotingEndpoint(string host, int port, string objIdentifier) {
            Host = host;
            Port = port;
            ObjIdentifier = objIdentifier;

            EndpointURL = BuildRemoteUrl(host, port, objIdentifier+port);

            // register tcp channel and service
            IDictionary dictionary = new Hashtable();
            dictionary["name"] = "tcp" + port;
            dictionary["port"] = port;
            TcpChannel = new TcpChannel(dictionary, null,null);
            ChannelServices.RegisterChannel(TcpChannel, false);
            RemotingServices.Marshal(this, objIdentifier + port, typeof(RemotingEndpoint));

            KnownServerRemotes = GetKnownServerRemotes();
        }

        private List<RemotingEndpoint> GetKnownServerRemotes() {
            var knownRemotes = new List<RemotingEndpoint>();

            for(var i = DefaultServerPort; i < DefaultServerPort+NumServers; i++) {
                if (i == Port) continue;
                string serverUrl = (BuildRemoteUrl(DefaultServerHost, i, "Server"+i));

                knownRemotes.Add(GetRemoteEndpoint(serverUrl));
            }
            return knownRemotes;
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

            foreach (var serverRemote in KnownServerRemotes)
            {
                messages.Add(SendMessageToRemote(serverRemote, message));

            }
            return messages;
        }

        public static string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        public abstract Message OnReceiveMessage(Message message);

        public abstract Message OnSendMessage(Message message);
    }
}
