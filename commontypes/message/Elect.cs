namespace CommonTypes.message {

    public enum ElectType { SET_MASTER, GET_MASTER }
    
    // A message sent to inform of a new master server
    [System.Serializable]
    public class Elect : Message {

        public string NewMasterURL { get; set; }
        public ElectType ElectType { get; private set; }

        public Elect(string srcRemoteURL, ElectType electType, string newMaster = null) : base(srcRemoteURL) {
            NewMasterURL = newMaster;
            ElectType = electType;
        }
        
    }
}
