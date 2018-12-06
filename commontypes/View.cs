using System;
using System.Collections.Generic;

namespace CommonTypes
{
    [Serializable]
    public class View
    {
        public HashSet<string> Nodes { get; }
        public long Version { get; }
        
        public View(IEnumerable<string> nodes, long version)
        {
            Nodes = new HashSet<string>(nodes);
            Version = version;
        }

        public int Size()
        {
            return Nodes.Count;
        }

    }
}