using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.EventStream
{
    public interface IServiceManager : IService
    {
        IServiceManager Add(IService service);
        IServiceManager Add<TService>() where TService : IService;
        Task Start(string serviceName, CancellationToken token = default(CancellationToken));
        Task Stop(string serviceName);
    }
}