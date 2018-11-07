﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class Client : RemotingEndpoint {
        private int clientRequestSeqNumber;
        public int ClientRequestSeqNumber { get; set; }

        private bool isBlockedFromSendingRequests;
        public bool IsBlockedFromSendingRequests { get; private set; }

        private Dictionary<int, Response> receivedResponses;
        public Dictionary<int, Response> ReceivedResponses { get; private set; }
        
        public Client() : this(defaultClientHost, defaultClientPort) { }

        public Client(string host, int port) : base(host, port, "Client") {
            receivedResponses = new Dictionary<int, Response>();
            clientRequestSeqNumber = 0;
        }

        public void Write(Tuple tuple) {
            if(isBlockedFromSendingRequests) {
                // TODO launch exception and return;
                return;
            }
            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL, RequestType.WRITE, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;
        }

        public void Read(Tuple tuple) {
            if (isBlockedFromSendingRequests) {
                // TODO launch exception and return;
                return;
            }
            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL,RequestType.READ, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;

            isBlockedFromSendingRequests = true;
        }


        public void Take(Tuple tuple) {
            if (isBlockedFromSendingRequests) {
                // TODO launch exception and return;
                return;
            }
            // remote exceptions?
            Request request = new Request(clientRequestSeqNumber, endpointURL, RequestType.TAKE, tuple);

            SendMessageToKnownServers(request);
            clientRequestSeqNumber++;

            isBlockedFromSendingRequests = true;
        }

        

        public override void OnReceiveMessage(Message message) {
            if (message.GetType().Equals(typeof(Response))){
                Response response = (Response)message;

                // NOTE: this assumes a server sends back the perfectly correct response that we wanted
                // if the request we sent out has already gotten the respective response ( same seqNumber in request and response of the client ) then ignore
                if (receivedResponses.Keys.Contains(response.Request.SeqNum)) {
                    return;
                }

                // if receives atleast one response for a blockingrequest then unlock ( can only send one and then block, so it will always unblock properly? )
                if(response.Request.RequestType.Equals(RequestType.READ) || response.Request.RequestType.Equals(RequestType.TAKE)){
                    isBlockedFromSendingRequests = false;

                    // TODO parse the response data from the request and do something with it


                }

                receivedResponses.Add(response.Request.SeqNum, response);
            }
        }

        public override void OnSendMessage(Message message) {
            throw new NotImplementedException();
        }
    }
}
