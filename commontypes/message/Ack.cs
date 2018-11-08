using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    [Serializable]
    public class Ack : Message {
        private Message _message; // original message the ack refers to
        public Message Message { get; set; }

        public Ack(string srcRemoteURL, Message message) : base(srcRemoteURL) {
            Message = message;
        }
    }
}
