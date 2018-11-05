namespace CommonTypes {
    // Allow a class extending from IRemoting to receive messages with Net Remoting
    public interface IRemoting {
        void OnReceiveMessage(Message message);
        void OnSendMessage(Message message);
        void RegisterTcpChannel();
        void RegisterService();
    }
}
