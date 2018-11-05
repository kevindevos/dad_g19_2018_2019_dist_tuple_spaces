using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes {
    public delegate void RemoteAsyncDelegate(Message message);
    // Allow a class extending from IRemoting to receive messages with Net Remoting
    public interface IRemoting {
        void OnReceiveMessage(Message message);
        void OnSendMessage(Message message);
        void RegisterTcpChannel();
        void RegisterService();
        IRemoting GetRemote(string host, int destPort, string objIdentifier);
    }
}
