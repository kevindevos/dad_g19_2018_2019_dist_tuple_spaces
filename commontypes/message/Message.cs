using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {

    public class Message {
        public int seqNum { get { return seqNum; } set { seqNum = value; } }
        public int clientId { get { return clientId; } set { clientId = value; } }
        public string data { get { return data; } set { data = value; } }

        public Message(int seqNum, int clientId, string data) {
            this.seqNum = seqNum;
            this.clientId = clientId;
            this.data = data;
        }

    }
}
