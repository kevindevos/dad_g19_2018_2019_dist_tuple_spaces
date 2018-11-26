using System;

namespace CommonTypes.message {
    [Serializable]
    public class Ack : Message {
        public Message Message { get; set; }

        public Ack(string srcRemoteURL, Message message) : base(srcRemoteURL) {
            Message = message;
        }
    }
}
