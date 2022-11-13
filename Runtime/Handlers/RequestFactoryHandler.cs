using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Handlers
{
    public class RequestFactoryHandler : IHandler
    {
        private readonly Func<IHandler> _handlerFactory;
        private readonly Dictionary<int, IHandler> _handlers;

        public RequestFactoryHandler(Func<IHandler> handlerFactory)
        {
            _handlerFactory = handlerFactory;
            _handlers = new Dictionary<int, IHandler>();
        }

        public Task<Request> OnRequest(Request value)
        {
            var handler = _handlerFactory();
            _handlers.Add(value.RequestId, handler);
            return handler.OnRequest(value);
        }

        public async Task<Response> OnReceive(Response value)
        {
            if (_handlers.TryGetValue(value.RequestId, out IHandler handler))
            {
                _handlers.Remove(value.RequestId);
                var result = await handler.OnReceive(value);
                return result;
            }

            return value;
        }
    }
}
