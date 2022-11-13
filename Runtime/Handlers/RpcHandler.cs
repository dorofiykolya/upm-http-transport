using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HttpTransport.Transports;
using OpenUGD;

namespace HttpTransport.Handlers
{
    public class RpcHandler : IHandler
    {
        private readonly Lifetime _lifetime;
        private readonly Dictionary<Key, Signal<object, object>> _map;

        public RpcHandler(Lifetime lifetime)
        {
            _lifetime = lifetime;
            _map = new Dictionary<Key, Signal<object, object>>();
        }

        public void SubscribeOnResponse<TResponse, TRequest>(Lifetime lifetime, Action<TResponse, TRequest> listener) where TRequest : class
        {
            var key = new Key
            {
                Request = typeof(TRequest),
                Response = typeof(TResponse)
            };
            Signal<object, object> signal;
            if (!_map.TryGetValue(key, out signal))
            {
                _map[key] = signal = new Signal<object, object>(_lifetime);
            }
            signal.Subscribe(lifetime, (o, b) =>
            {
                var requestType = typeof(TRequest);
                var responseType = typeof(TResponse);
                listener((TResponse)o, (TRequest)b);
            });
        }

        public Task<Request> OnRequest(Request value)
        {
            return Task.FromResult(value);
        }

        public Task<Response> OnReceive(Response value)
        {
            if (value.ResponseCode == 200)
            {
                var key = new Key
                {
                    Request = value.Request.RequestType,
                    Response = value.ResponseType
                };
                Signal<object, object> signal;
                if (_map.TryGetValue(key, out signal))
                {
                    signal.Fire(value.Content, value.Request.RequestObject);
                }
            }
            return Task.FromResult(value);
        }

        private struct Key : IEquatable<Key>
        {
            public Type Request;
            public Type Response;

            public bool Equals(Key other)
            {
                return Equals(Request, other.Request) && Equals(Response, other.Response);
            }

            public override bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Request != null ? Request.GetHashCode() : 0) * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                }
            }
        }
    }
}
