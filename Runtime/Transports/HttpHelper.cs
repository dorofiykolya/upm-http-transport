namespace HttpTransport.Transports
{
    public static class HttpHelper
    {
        public static string GetHeaderAuthorization(string token) => $"Bearer {token}";
        public static string ContentTypeApplicationJson => "application/json";
    }
}
