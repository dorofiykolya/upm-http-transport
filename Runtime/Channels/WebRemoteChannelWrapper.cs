using System.Collections.Generic;
using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Channels
{
    public class WebRemoteChannelWrapper : IChannel
    {
        private readonly WebRemoteChannel _channel;

        public WebRemoteChannelWrapper(WebRemoteChannel channel)
        {
            _channel = channel;
        }

        public string Uri
        {
            get => _channel.Uri;
            set => _channel.Uri = value;
        }

        public bool DisableConnection { get; set; }

        public Task<Response> Send(Request request)
        {
            if (DisableConnection)
            {
                return Task.FromResult(new Response(request, new Dictionary<string, string>(), null, 0, true,
                        "connection disabled"));
            }
            return _channel.Send(request);
        }
    }
}
