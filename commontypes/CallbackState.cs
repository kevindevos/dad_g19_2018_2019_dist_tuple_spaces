using CommonTypes.message;

namespace CommonTypes
{
    public class CallbackState
    {
        public Message OriginalMessage { get; }
        public string RemoteUrl { get; }

        public CallbackState(Message originalMessage, string remoteUrl)
        {
            OriginalMessage = originalMessage;
            RemoteUrl = remoteUrl;
        }
    }
}