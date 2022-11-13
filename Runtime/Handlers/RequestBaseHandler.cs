using System;
using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Handlers
{
    class RequestBaseHandler : IHandler
    {
        public Task<Request> OnRequest(Request value)
        {
            throw new NotImplementedException();
        }

        public Task<Response> OnReceive(Response value)
        {
            throw new NotImplementedException();
        }
    }
}
