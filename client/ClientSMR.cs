using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.message;
using Tuple = CommonTypes.tuple.Tuple;

namespace ClientNamespace {
    public class ClientSMR : Client{
        public ClientSMR(string host, int port) {
            SendMessageDel = new SendMessageDelegate(SendMessageToRandomServer);
        }

        public ClientSMR() :this(DefaultClientHost, DefaultClientPort){
        }

      
     
        

       

        
    }
}
