namespace HttpTransport.Transports
{
    public interface IRemoteRequest
    {
        byte[] Data { get; }
        object GetData(string name);
        object ContentType { get; }
    }
}
