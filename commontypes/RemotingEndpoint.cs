using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.message;
using System.Collections;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CommonTypes {
    public delegate Message RemoteAsyncDelegate(Message message);

    public abstract class RemotingEndpoint : MarshalByRefObject  {
        protected const string DefaultServerHost = "localhost";
        protected const int DefaultServerPort = 8080;

        protected const string DefaultClientHost = "localhost";
        protected const int DefaultClientPort = 8070;

        private string ObjIdentifier { get; }

        public List<RemotingEndpoint> View;

        public string EndpointURL { get; }

        private TcpChannel TcpChannel{ get; set; }

        protected int Port { private get; set; }

        private string Host { get; }

        protected RemotingEndpoint(string remoteUrl, List<string> knownServerUrls=null) : this(remoteUrl)
        {
            if (knownServerUrls is null)
            {
                var inputFile =
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                        throw new FileNotFoundException(), @"Resources/bootstrapServers.txt");                
                
                knownServerUrls = new List<string>(File.ReadAllLines(inputFile));
            }

            View = BuildView(knownServerUrls);
            Bootstrap();
        }
        
        private RemotingEndpoint(string remoteUrl){
            string[] splitUrl = SplitUrlIntoHostPortAndId(remoteUrl);

            if (splitUrl.Count() != 3){
                throw new Exception("Invalid remote Url passed to constructor.");
            }
            
            Host = splitUrl[0];
            Port = int.Parse(splitUrl[1]);
            ObjIdentifier = splitUrl[2];

            EndpointURL = remoteUrl;
            IDictionary dictionary = new Hashtable();
            dictionary["name"] = "tcp" + Port;
            dictionary["port"] = Port;
            TcpChannel = new TcpChannel(dictionary, null,null);
            ChannelServices.RegisterChannel(TcpChannel, false);
            RemotingServices.Marshal(this, ObjIdentifier, typeof(RemotingEndpoint));
        }

        // check which servers are up
        private void Bootstrap()
        {
            var liveServers = new List<RemotingEndpoint>();
            var liveServersString = "Live servers: \n";
            foreach (var serverRemote in View)
            {
                try
                {
                    serverRemote.Ping();
                    if (serverRemote.EndpointURL == EndpointURL) continue;
                    
                    liveServers.Add(serverRemote);
                    liveServersString += "\t"+ serverRemote.EndpointURL + "\n";
                }
                catch (Exception e)
                {
                    //do nothing
                }
            }
            View = liveServers;
            Console.WriteLine(liveServersString);
        }


        public static RemotingEndpoint GetRemoteEndpoint(string url) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint), url);

            return remote;
        }

        private List<RemotingEndpoint> BuildView(List<string> knownServerUrls) {
            var view = new List<RemotingEndpoint>();

            foreach (var serverUrl in knownServerUrls)
            {
                view.Add(GetRemoteEndpoint(serverUrl));
            }
            
            return view;
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
                throw new SocketException();
            }
        }

        public Message SendMessageToRemoteURL(string remoteURL, Message message) {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remoteURL);
            return SendMessageToRemote(remotingEndpoint, message);
        }

        public void SendMessageToView(List<string> remotingURLS, Message message) {
            foreach (string ru in remotingURLS) {
                SendMessageToRemoteURL(ru, message);
            }
        }

        public void SendMessageToView(List<RemotingEndpoint> servers, Message message) {
            foreach(RemotingEndpoint re in servers) {
                SendMessageToRemote(re, message);
            }
        }
        

        protected Message SendMessageToRandomServer(Message message) {
            var random = new Random();
            var i = random.Next(0, View.Count);
            return SendMessageToRemote(View[i], message);
        }

        public static string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        protected string[] SplitUrlIntoHostPortAndId(string url){
            return url.Substring(6).Split(new char[]{':', '/'});
        }

        public List<RemotingEndpoint> GetView(){
            return View;
        }

        public abstract Message OnReceiveMessage(Message message);

        public abstract Message OnSendMessage(Message message);

        public void Ping()
        {
        }
    }
}
