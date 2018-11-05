﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    // An order is a message sent by a master to other normal servers, to perform a certain action/request
    public class Order : Message{
        private Request request;

        public Request Request { get; private set; }

        public Order(Request request) {
            this.Request = request;
        }

    }
}
