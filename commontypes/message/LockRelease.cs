using System;

namespace CommonTypes.message{
    /**
     * Sent by the XL Client to ask a server to release the locks issued by a take from a request
     * with sequence number TakeRequestSeqNum
     */
    [Serializable]
    public class LockRelease : Message{
        public int TakeRequestSeqNum{ get; }
        
        public LockRelease(string srcRemoteURL, int takeRequestSeqNum) : base(srcRemoteURL){
            TakeRequestSeqNum = takeRequestSeqNum;
        }
        
        
    }
} 