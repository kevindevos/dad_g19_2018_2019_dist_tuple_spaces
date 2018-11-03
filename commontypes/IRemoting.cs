﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.server {
    // Allow a class extending from IRemoting to receive messages with Net Remoting
    public interface IRemoting {
        Message OnReceiveMessage(Message message);
        void RegisterTcpChannel();
        void RegisterService();
    }
}