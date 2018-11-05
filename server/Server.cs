﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.server;
using Tuple = CommonTypes.Tuple;

namespace ServerNamespace{
    public class Server : MarshalByRefObject, IRemoting {
        // the server's functionality, can be changed when upgrading to or downgrading from MasterServerBehaviour
        private int serverPort;
        private TupleSpace tupleSpace;
        private TcpChannel tcpChannel;


        public void decide() {
            lock (requestList) {
                // TODO
                // decide from the list of requests if server can do something or not
            }
        }

        // A dictionary containing the most recent sequence numbers of the most recent requests of each client.  <clientRemoteURL, SeqNum>
        public Dictionary<string, int> mostRecentClientRequestSeqNumbers;

        // A list of requests the server receives, defines the order 
        // for a FIFO order process requests from index 0 and do RemoveAt(0)
        public List<Request> requestList;

        public ServerBehaviour behaviour;
        public TupleSpace TupleSpace { get; private set; }


        static void Main(string[] args) {
            Server server = new Server();
            server.RegisterTcpChannel();
            server.RegisterService();

            Console.WriteLine("Server Started, press <enter> to leave.");
            Console.ReadLine();
        }

        public Server(){
            this.tupleSpace = new TupleSpace();
            this.behaviour = new ServerBehaviour(this);
            this.serverPort = 8086; // Default server port
        }

        public Server(int serverPort) {
            this.tupleSpace = new TupleSpace();
            this.behaviour = new ServerBehaviour(this);
            this.serverPort = serverPort; 
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
