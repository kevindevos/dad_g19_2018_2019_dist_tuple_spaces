using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using CommonTypes.message;

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

        public View View;
        private TcpChannel TcpChannel{ get; set; }
        private string Host { get; }
        private int Port { get; set; }
        private string ObjIdentifier { get; }
        public string EndpointURL { get; }

        protected readonly ConcurrentDictionary<Message, ReplyResult> ReplyResultQueue;
        private readonly Dictionary<Message, object> _waitLocks;
        private readonly object _freezeLock;
        

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
            
            ReplyResultQueue = new ConcurrentDictionary<Message, ReplyResult>();
            _waitLocks = new Dictionary<Message, object>();
            
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
            if (TcpChannel == null) return;
            
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

        // send and ignore result
        // can throw exception
        public IAsyncResult NewSendMessageToRemoteURL(string remoteURL, Message message) {
            var remotingEndpoint = GetRemoteEndpoint(remoteURL);
            
            try {
                RemoteAsyncDelegate remoteDel = remotingEndpoint.OnReceiveMessage;
                return remoteDel.BeginInvoke(message, delegate(IAsyncResult ar)
                {
                    var result = (AsyncResult) ar;
                    var caller = (RemoteAsyncDelegate) result.AsyncDelegate;
                    caller.EndInvoke(ar);
                }, null);
            }
            catch(Exception e) {
                Console.WriteLine("Server at " + remoteURL + " is unreachable.");
                throw;
            }
        }

        public List<IAsyncResult> NewSendMessageToView(IEnumerable<string> remotingUrls, Message message) {
            var asyncResults = new List<IAsyncResult>();
            
            foreach (var remoteURL in remotingUrls) {
                try
                {
                    var ar = NewSendMessageToRemoteURL(remoteURL, message);
                    asyncResults.Add(ar);
                }
                catch (Exception e)
                {
                    // ignore failed server, continue sending message to the others
                }
            }
            return asyncResults;

        }
        
        
        public void SendMessageToView(IEnumerable<string> remotingUrls, Message message) {
            foreach (var ru in remotingUrls) {
                SendMessageToRemoteURL(ru, message);
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


        public void MulticastMessageWaitAll(IEnumerable<string> view, Message message)
        {
            MulticastMessage(view, message, WaitAllCallback);
        }
        public void MulticastMessageWaitAny(IEnumerable<string> view, Message message)
        {
            MulticastMessage(view, message, WaitAnyCallback);
        }

        public void SingleCastMessage(string remoteUrl, Message message)
        {
            var replyResult = new ReplyResult();
            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryAdd(message, replyResult);
            }
            
            try
            {
                lock (ReplyResultQueue)
                {
                    replyResult.AddRemoteUrl(remoteUrl);
                }
                MulticastSingleUrl(remoteUrl, message, WaitAnyCallback);
            }
            catch (Exception e)
            {
                // if connection failed, it means it's dead. moving on...
                lock (ReplyResultQueue){
                    replyResult.RemoveRemoteUrl(remoteUrl); //TODO is this necessary, or does the try do some rollback?
                }
                throw;
            }
            
        }
        
        private void MulticastMessage(IEnumerable<string> view, Message message, AsyncCallback asyncCallback)
        {
            var replyResult = new ReplyResult();
            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryAdd(message, replyResult);
            }
            
            foreach (var remoteUrl in view)
            {
                try
                {
                    lock (ReplyResultQueue)
                    {
                        replyResult.AddRemoteUrl(remoteUrl);
                    }
                    MulticastSingleUrl(remoteUrl, message, asyncCallback);
                }
                catch (Exception e)
                {
                    // if connection failed, it means it's dead. moving on...
                    lock (ReplyResultQueue){
                        replyResult.RemoveRemoteUrl(remoteUrl); //TODO is this necessary, or does the try do some rollback?
                    }
                }
            }
        }

        private void MulticastSingleUrl(string remoteURL, Message message, AsyncCallback asyncCallback) {
            var remotingEndpoint = GetRemoteEndpoint(remoteURL);
            
            try {
                RemoteAsyncDelegate remoteDel = remotingEndpoint.OnReceiveMessage;
                CallbackState state = new CallbackState(message, remoteURL);
                remoteDel.BeginInvoke(message, asyncCallback, state);
            }
            catch(Exception e) {
                Console.WriteLine("Server at " + remoteURL + " is unreachable.");
                throw;
            }
        }
        
        
        
        // Callbacks
        //
        private void WaitAllCallback(IAsyncResult asyncResult)
        {
            AsyncResult ar = (AsyncResult)asyncResult;
            RemoteAsyncDelegate remoteDel = (RemoteAsyncDelegate)ar.AsyncDelegate;
            Message responseMessage = remoteDel.EndInvoke(asyncResult);
            CallbackState state = (CallbackState)ar.AsyncState;
            
            Message originalMessage = state.OriginalMessage;
            string remoteUrl = state.RemoteUrl;

            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryGetValue(originalMessage, out var replyResult);
                if (replyResult == null) return;
                
                // save the result
                replyResult.AddResult(remoteUrl, responseMessage);
                
                // if ZERO waiting replies, notify the caller (the one who initiated the multi-cast)
                if (replyResult.NWaitingReply() == 0)
                {
                    PulseMessage(originalMessage);
                }
            }
        }

        private void WaitAnyCallback(IAsyncResult asyncResult)
        {
            AsyncResult ar = (AsyncResult)asyncResult;
            RemoteAsyncDelegate remoteDel = (RemoteAsyncDelegate)ar.AsyncDelegate;
            Message responseMessage = remoteDel.EndInvoke(asyncResult);
            CallbackState state = (CallbackState)ar.AsyncState;
            
            Message originalMessage = state.OriginalMessage;
            string remoteUrl = state.RemoteUrl;

            lock (ReplyResultQueue)
            {
                ReplyResultQueue.TryGetValue(originalMessage, out var replyResult);
                if (replyResult == null) return;
                
                // save the result
                replyResult.AddResult(remoteUrl, responseMessage);
                
                // if ANY result, notify the caller (the one who initiated the multi-cast)
                if (replyResult.NResults() > 0)
                {
                    PulseMessage(originalMessage);
                }
            }
        }
        
        // Wait and Pulse
        private void PulseMessage(Message originalMessage)
        {
            _waitLocks.TryGetValue(originalMessage, out var messageLock);
            if (messageLock != null)
            {
                // this lock is necessary to do the pulse
                lock (messageLock)
                {
                    Monitor.Pulse(messageLock);
                }
            }
        }
        public void WaitMessage(Message originalMessage, IEnumerable<string> viewNodes = null, int timeout=Timeout.Infinite)
        {
            if (viewNodes != null && !viewNodes.Any()) return;
            
            _waitLocks.Add(originalMessage, new object());
            _waitLocks.TryGetValue(originalMessage, out var messageLock);
            if (messageLock != null)
            {
                // this lock is necessary to do the wait
                lock (messageLock)
                {
                    Monitor.Wait(messageLock, timeout);
                }
            }
        }

        public void Crash()
        {
            DisposeChannel();
        }

        public void Freeze()
        {
            Monitor.TryEnter(_freezeLock, 10000);
        }
        
        public void Unfreeze()
        {
            lock (_freezeLock)
            {
                Monitor.PulseAll(_freezeLock);
            }
        }

    }
}