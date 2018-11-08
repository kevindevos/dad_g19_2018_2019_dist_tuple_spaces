using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {

    // A message sent to inform of a new master server
    [System.Serializable]
    public class Elect : Message {
        private string _newMasterURL;
        public string NewMasterURL { get; private set; }

        public Elect(string srcRemoteURL, string newMaster) : base(srcRemoteURL) {
            NewMasterURL = newMaster;
        }
    }
}
