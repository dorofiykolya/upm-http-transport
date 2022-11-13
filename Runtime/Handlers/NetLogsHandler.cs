using System.Threading.Tasks;
using HttpTransport.Transports;
using HttpTransport;

namespace HttpTransport.Handlers
{
    public class NetLogsHandler : IHandler
    {
        private readonly IHttpTransportLogs _logs;

        public NetLogsHandler(IHttpTransportLogs logs)
        {
            _logs = logs;
        }

        public Task<Request> OnRequest(Request value)
        {
            _logs.Push(value.RequestId, value.Uri, value, value.Content, PacketLogIO.Call);
            return Task.FromResult(value);
        }

        public Task<Response> OnReceive(Response value)
        {
            _logs.Push(value.RequestId, $"{value.Request.Uri} [{value.ResponseCode}:{(int)value.ResponseCode}]", value, value.Content, PacketLogIO.Response);
            return Task.FromResult(value);
        }
    }
}
