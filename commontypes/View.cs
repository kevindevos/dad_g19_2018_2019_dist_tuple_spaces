using System;
using System.Collections.Generic;

namespace CommonTypes
{
    [Serializable]
    public class View
    {
        public HashSet<string> Nodes { get; private set; }
        public long Version { get; private set;  }
        
        public View(IEnumerable<string> nodes, long version)
        {
            Nodes = new HashSet<string>(nodes);
            Version = version;
        }

        public int Size()
        {
            return Nodes.Count;
        }

        public HashSet<string> Join(IEnumerable<string> nodes)
        {
            var remoteUrls = Nodes;
            remoteUrls.UnionWith(nodes);

            Nodes = remoteUrls;
            Version += 1;

            return Nodes;
        }

        public HashSet<string> Set(IEnumerable<string> nodes)
        {
            foreach (var node in nodes)
            {
                Nodes.Remove(node);
            }
    
            Version += 1;
            
            return Nodes;
        }

    }
}