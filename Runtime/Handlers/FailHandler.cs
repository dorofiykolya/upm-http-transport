using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Handlers
{
    public class FailHandler : IHandler
    {
        public Task<Request> OnRequest(Request value)
        {
            return Task.FromResult(value);
        }

        public Task<Response> OnReceive(Response value)
        {
            if (value.IsFail || value.IsNetworkError)
            {
                Pipeline.Break();
            }

            return Task.FromResult(value);
        }
    }
}
