using System.Threading.Tasks;

namespace HttpTransport.Transports
{
    public interface IHandler
    {
        Task<Request> OnRequest(Request value);
        Task<Response> OnReceive(Response value);
    }
}
