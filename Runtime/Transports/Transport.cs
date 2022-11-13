using System;
using System.Threading.Tasks;
using OpenUGD;

namespace HttpTransport.Transports
{
    public class Transport : IDisposable
    {
        public static ISetChannel Factory(Lifetime lifetime)
        {
            return new TransportFactory(lifetime);
        }

        private readonly Lifetime.Definition _definition;
        private IChannel _channel;
        private Pipeline _pipeline;

        public Lifetime Lifetime { get; }

        private Transport(Lifetime.Definition definition)
        {
            _definition = definition;
            Lifetime = definition.Lifetime;
        }

        public async Task<Response> Send(Request value)
        {
            var result = await _pipeline.Send(value);
            if (result == null || Lifetime.IsTerminated) return null;
            var response = await _channel.Send(result);
            if (Lifetime.IsTerminated) return null;
            return await _pipeline.Receive(response);
        }

        private void SetChannel(IChannel channel)
        {
            _channel = channel;
        }

        private void SetPipeline(Action<Lifetime, Pipeline> pipeline)
        {
            _pipeline = new Pipeline(this, _channel);
            pipeline(Lifetime, _pipeline);
        }

        public void Dispose()
        {
            _definition.Terminate();
        }

        private class TransportFactory : ISetChannel, ISetHandler, ISetBuilder
        {
            private readonly Lifetime _lifetime;
            private ChannelFactory _channel;
            private Action<Lifetime, IPipeline> _pipeline;

            public TransportFactory(Lifetime lifetime)
            {
                _lifetime = lifetime;
            }

            public ISetHandler Channel(ChannelFactory channelFactory)
            {
                _channel = channelFactory;
                return this;
            }

            public ISetBuilder Handler(Action<Lifetime, IPipeline> pipeline)
            {
                _pipeline = pipeline;
                return this;
            }

            public Transport Build()
            {
                var transport = new Transport(_lifetime.DefineNested());
                transport.SetChannel(_channel(transport.Lifetime));
                transport.SetPipeline(_pipeline);
                return transport;
            }
        }
    }
}
