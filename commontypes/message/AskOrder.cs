using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    // This will be a type of message sent by a server to the master ( everyone but only master replies) to ask for a missing order with specific sequence number
    public class AskOrder : Message {
        private int _wantedSequenceNumber;
        public int WantedSequenceNumber { get => _wantedSequenceNumber; private set => _wantedSequenceNumber = value; }

        public AskOrder(string srcRemoteURL, int wantedOrderSequenceNumber) : base(srcRemoteURL) {
            this.WantedSequenceNumber = wantedOrderSequenceNumber;
        }

    }
}
