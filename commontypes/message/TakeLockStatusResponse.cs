using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes.message {
    public class TakeLockStatusResponse : Response {
        public bool LockAccepted { get; private set; }

        public TakeLockStatusResponse(Request request, string srcRemoteURL, bool wasLockAccepted) : base(request, null, srcRemoteURL) {
            LockAccepted = wasLockAccepted;
        }
    }
}
