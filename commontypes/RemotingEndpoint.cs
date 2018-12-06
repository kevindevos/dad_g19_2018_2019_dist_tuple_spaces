using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes.message;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters;

namespace CommonTypes {
    public delegate Message RemoteAsyncDelegate(Message message);
    public delegate void PingDelegate();
    public delegate HashSet<string> JoinViewDelegate(HashSet<string> remotingEndpoints);
    public delegate View GetViewDelegate();

    public abstract class RemotingEndpoint : MarshalByRefObject  {
        protected const string DefaultServerHost = "localhost";
        protected const int DefaultServerPort = 8080;

        protected const string DefaultClientHost = "localhost";
        protected const int DefaultClientPort = 8070;

        private string ObjIdentifier { get; }

        public View View;

        public string EndpointURL { get; }

        private TcpChannel TcpChannel{ get; set; }

        protected int Port { private get; set; }

        private string Host { get; }

        protected RemotingEndpoint(string remoteUrl, IEnumerable<string> knownServerUrls=null) : this(remoteUrl)
        {
            if (knownServerUrls is null)
            {
                var inputFile =
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                        throw new FileNotFoundException(), @"Resources/bootstrapServers.txt");                
                
                knownServerUrls = new List<string>(File.ReadAllLines(inputFile));
            }

            View = new View(knownServerUrls, 0);
            
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
            
            /*
             * this code is necessary to pass the tests with client/server.
             * without this some security level error appeared when trying to call
             */
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            //
            
            TcpChannel = new TcpChannel(dictionary, null,provider);
            ChannelServices.RegisterChannel(TcpChannel, false);
            RemotingServices.Marshal(this, ObjIdentifier, typeof(RemotingEndpoint));
        }

        public void DisposeChannel()
        {
            TcpChannel.StopListening(null);
            RemotingServices.Disconnect(this);
            ChannelServices.UnregisterChannel(TcpChannel);
            TcpChannel = null;        
        }

        // check which servers are up from the seeds
        private void Bootstrap()
        {
            var liveServers = new HashSet<string>();
            foreach (var remotingEndpointUrl in View.Nodes)
            {
                try
                {
                    DoPing(remotingEndpointUrl);
                    if (remotingEndpointUrl == EndpointURL) continue;
                    
                    liveServers.Add(remotingEndpointUrl);
                }
                catch (Exception e)
                {
                    //do nothing
                }
            }
            View = new View(liveServers, 0);
            
            // RecursiveJoin(View); //server only
            RecursiveGetView(View.Nodes);
            PrintCurrentView();
        }

        private void PrintCurrentView()
        {
            Console.WriteLine("\n[" + ObjIdentifier + "] Current view:");
            foreach (var serverRemote in View.Nodes)
            {
                Console.WriteLine("\t"+ serverRemote);
            }

            if (View.Size() == 0)
            {
                Console.WriteLine("\t<empty>");
            }
        }

        // Recursive join every server and update view
        public void RecursiveJoinView(IEnumerable<string> view)
        {
            var toJoin = view;
            var self = new HashSet<string>() {EndpointURL};
            var unionOfReturnedView = new HashSet<string>();
            var joined = new HashSet<string>();
            
            while (true)
            {
                foreach (var serverRemoteUrl in toJoin)
                {
                    try
                    {
                        var returnedView = DoJoinView(serverRemoteUrl, self);
                        unionOfReturnedView.UnionWith(returnedView);
                    }
                    catch (Exception e)
                    {
                        //do nothing...
                    }
                }

                joined.UnionWith(toJoin);
                unionOfReturnedView.ExceptWith(joined);
                unionOfReturnedView.ExceptWith(self);
                toJoin = new HashSet<string>(unionOfReturnedView);
                if (!toJoin.Any())
                    break;
            }

            JoinView(joined);
        }
        
        // recursive get view
        private void RecursiveGetView(HashSet<string> view)
        {
            var toQuery = view;
            
            var self = new HashSet<string>() {EndpointURL};
            var unionOfReturnedView = new HashSet<string>();
            var queried = new HashSet<string>();
            
            while (true)
            {
                foreach (var serverRemote in toQuery)
                {
                    try
                    {
                        var returnedView = DoGetView(serverRemote);
                        unionOfReturnedView.UnionWith(returnedView.Nodes);
                    }
                    catch (Exception e)
                    {
                        //do nothing...
                    }
                }

                queried.UnionWith(toQuery);
                unionOfReturnedView.ExceptWith(queried);
                unionOfReturnedView.ExceptWith(self);
                toQuery = new HashSet<string>(unionOfReturnedView);
                if (toQuery.Count == 0)
                    break;
            }

            JoinView(queried);
        }


        public static RemotingEndpoint GetRemoteEndpoint(string url) {
            RemotingEndpoint remote = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint), url);
            
            return remote;
        }

        private HashSet<RemotingEndpoint> BuildView(List<string> knownServerUrls) {
            var view = new HashSet<RemotingEndpoint>();

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
                //ar.AsyncWaitHandle.WaitOne();
                // This should not be necessary, since EndInvoke already waits for completion
                // unless we want to provide a timeout a do something with it
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

        public void SendMessageToView(IEnumerable<string> remotingURLS, Message message) {
            foreach (string ru in remotingURLS) {
                SendMessageToRemoteURL(ru, message);
            }
        }

        public void SendMessageToView(IEnumerable<RemotingEndpoint> servers, Message message) {
            foreach(RemotingEndpoint re in servers) {
                SendMessageToRemote(re, message);
            }
        }
        
        public static string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }

        protected string[] SplitUrlIntoHostPortAndId(string url){
            return url.Substring(6).Split(new char[]{':', '/'});
        }


        public abstract Message OnReceiveMessage(Message message);

        
        
        
        private void DoPing(string remotingEndpointUrl)
        {
            var remotingEndpoint = GetRemoteEndpoint(remotingEndpointUrl);
            
            PingDelegate pingDelegate = remotingEndpoint.Ping;
            var asyncResult = pingDelegate.BeginInvoke(null, null);
            pingDelegate.EndInvoke(asyncResult);
        }

        private HashSet<string> DoJoinView(string remotingEndpointUrl, HashSet<string> view)
        {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remotingEndpointUrl);
            
            JoinViewDelegate joinViewDelegate = remotingEndpoint.JoinView;
            var asyncResult = joinViewDelegate.BeginInvoke(view, null, null);
            return joinViewDelegate.EndInvoke(asyncResult);
        }
        
        private View DoGetView(string remotingEndpointUrl)
        {
            RemotingEndpoint remotingEndpoint = GetRemoteEndpoint(remotingEndpointUrl);
            
            GetViewDelegate getViewDelegate = remotingEndpoint.GetView;
            var asyncResult = getViewDelegate.BeginInvoke(null, null);
            return getViewDelegate.EndInvoke(asyncResult);
        }

        
        
        public void Ping()
        {
        }

        public HashSet<string> JoinView(IEnumerable<string> newEndpointUrl)
        {
            var remoteUrls = View.Nodes;
            remoteUrls.UnionWith(newEndpointUrl);
            
            View = new View(remoteUrls, View.Version+1);

            return remoteUrls;
        }
        
        public View GetView()
        {
            return View;
        }

    }
}