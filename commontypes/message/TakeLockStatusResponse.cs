namespace CommonTypes.message {
    public class TakeLockStatusResponse : Response {
        public bool LockAccepted { get; private set; }

        public TakeLockStatusResponse(Request request, string srcRemoteURL, bool lockAccepted) : base(request, null, srcRemoteURL) {
            LockAccepted = lockAccepted;
        }
    }
}
