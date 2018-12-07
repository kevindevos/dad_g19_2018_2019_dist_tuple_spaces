
using System;
using Tuple = CommonTypes.tuple.Tuple;

namespace CommonTypes.message{
    /**
     * Used for the client to ask servers to remove the tuple to finish a take request in XL
     */
    [Serializable]
    public class TakeRemove : Message{
        // tuple to be removed
        public Tuple SelectedTuple { get; }
        
        // request sequence number of the corresponding take
        public int TakeRequestSeqNumber{ get; }
        
        public TakeRemove(string srcRemoteURL, Tuple selectedTuple, int takeRequestSeqNum) : base(srcRemoteURL){
            SelectedTuple = selectedTuple;
            TakeRequestSeqNumber = takeRequestSeqNum;
        }
    }
}