namespace CommonTypes {
    // Allow a class extending from IRemoting to receive messages with Net Remoting
    public interface IRemoting {
        Message OnReceiveMessage(Message message);
        Message OnSendMessage(Message message);
        void RegisterTcpChannel();
        void RegisterService();
    }
}
