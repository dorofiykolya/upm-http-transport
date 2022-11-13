using OpenUGD;

namespace HttpTransport.Transports
{
    public delegate IChannel ChannelFactory(Lifetime lifetime);

    public interface ISetChannel
    {
        ISetHandler Channel(ChannelFactory channelFactory);
    }
}
