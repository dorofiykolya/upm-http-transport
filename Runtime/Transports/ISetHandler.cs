using System;
using OpenUGD;

namespace HttpTransport.Transports
{
    public interface ISetHandler
    {
        ISetBuilder Handler(Action<Lifetime, IPipeline> pipeline);
    }
}
