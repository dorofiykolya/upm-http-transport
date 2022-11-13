using System.Threading.Tasks;

namespace HttpTransport.Transports
{
    public interface IChannel
    {
        Task<Response> Send(Request request);
    }
}
