using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {

    public class Message {
        public int seqNum { get { return seqNum; } set { seqNum = value; } }
        public string clientRemoteURL { get { return clientRemoteURL; } set { clientRemoteURL = value; } }
        public Tuple tuple { get { return tuple; } set { tuple = value; } }

        public Message(int seqNum, string clientRemoteURL, Tuple tuple) {
            this.seqNum = seqNum;
            this.clientRemoteURL = clientRemoteURL;
            this.tuple = tuple;
        }

    }
}
