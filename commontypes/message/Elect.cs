namespace CommonTypes.message {

    // A message sent to inform of a new master server
    [System.Serializable]
    public class Elect : Message {
        public string NewMasterURL { get; private set; }

        public Elect(string srcRemoteURL, string newMaster) : base(srcRemoteURL) {
            NewMasterURL = newMaster;
        }
    }
}
