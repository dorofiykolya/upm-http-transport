namespace HttpTransport
{
    public interface IHttpTransportLogs
    {
        PacketLog[] Collection { get; }
        void Push(long id, string key, object value, object content, PacketLogIO io);
        void Clear();
    }
}
