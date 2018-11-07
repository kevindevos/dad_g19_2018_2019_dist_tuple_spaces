namespace CommonTypes.message {
    public abstract class Message {
        private string _srcRemoteURL;
        public string SrcRemoteURL { get => _srcRemoteURL; set => _srcRemoteURL = value; }

        public Message(string srcRemoteURL) {
            SrcRemoteURL = srcRemoteURL;
        }
    }
}
