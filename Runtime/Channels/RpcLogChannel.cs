using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Channels
{
    public class RpcLogChannel : IChannel
    {
        private readonly IChannel _channel;
        private readonly IHttpTransportLogs _logs;

        public RpcLogChannel(IChannel channel, IHttpTransportLogs logs)
        {
            _channel = channel;
            _logs = logs;
        }

        public async Task<Response> Send(Request request)
        {
            _logs.Push(request.RequestId, request.Uri, request, request.Content, PacketLogIO.Call);
            var response = await _channel.Send(request);
            _logs.Push(response.RequestId,
                    $"{response.Request.Uri} [{response.ResponseCode}:{(int)response.ResponseCode}]", response,
                    response.Debug, PacketLogIO.Response);
            return response;
        }
    }
}
