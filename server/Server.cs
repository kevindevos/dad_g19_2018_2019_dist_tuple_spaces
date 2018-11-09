<<<<<<< HEAD
 using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace{
    public class Server : RemotingEndpoint, IRemoting {
        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        public const int defaultServerPort = 8086;
        public const string defaultServerHost = "localhost";

        private int serverPort;
        private string serverHost;

        private TupleSpace tupleSpace;
        private TcpChannel tcpChannel;

        private int lastOrderSequenceNumber;
        public int LastOrderSequenceNumber { get; set; }

        private List<IRemoting> otherServers;
        public List<IRemoting> OtherServers { get; private set; }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public ConcurrentDictionary<string, Request> lastExecutedClientRequests;

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> requestList;
        public List<Request> RequestList { get; set; }

        public ServerBehaviour behaviour;
        public TupleSpace TupleSpace { get; private set; }

        static void Main(string[] args) {
            Server server = new Server();
            server.RegisterTcpChannel();
            server.RegisterService();

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }

        public Server() : this(defaultServerHost, defaultServerPort) { }

        public Server(string host, int port) {
            this.tupleSpace = new TupleSpace();
            this.behaviour = new ServerBehaviour(this);
            this.serverPort = port;
            this.serverHost = host;
            otherServers = new List<IRemoting>();
        }

        public void SaveRequest(Request request) {
            lock (requestList) {
                requestList.Add(request);
            }
        }

        public void DeleteRequest(Request request) {
            lock (requestList) {
                requestList.Remove(request);
            }
        }


        public void RegisterTcpChannel() {
            tcpChannel = new TcpChannel(serverPort);
            ChannelServices.RegisterChannel(tcpChannel, false);
        }

        public void RegisterService() {
            RemotingServices.Marshal(this, "Server", typeof(Server));
        }

        public void UpgradeToMaster() {
            this.behaviour = new MasterServerBehaviour(this);
        }

        public void DowngradeToNormal() {
            this.behaviour = new ServerBehaviour(this);
        }

        public void OnReceiveMessage(Message message) {
            behaviour.OnReceiveMessage(message);
        }

        public void OnSendMessage(Message message) {
            throw new NotImplementedException();
        }

        public string BuildRemoteUrl(string host, int port, string objIdentifier) {
            return "tcp://" + host + ":" + port + "/" + objIdentifier;
        }
    }

}
=======
﻿
>>>>>>> 8f6f601164ee3244ab3007ec0a0856b7274d638e
