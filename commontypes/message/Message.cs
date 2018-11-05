namespace CommonTypes.message {
    public abstract class Message {
        private string srcRemoteUrl;
        public string SrcRemoteURL { get; private set; }

        public Message(string srcRemoteURL) {
            this.SrcRemoteURL = srcRemoteUrl;
        }
    }
}
