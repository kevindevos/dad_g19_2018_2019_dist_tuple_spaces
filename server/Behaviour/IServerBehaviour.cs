using CommonTypes;

namespace ServerNamespace.Behaviour
{
    public interface IServerBehaviour
    {
        Message OnReceiveMessage(Message message);
        Message OnSendMessage(Message message);
    }
}