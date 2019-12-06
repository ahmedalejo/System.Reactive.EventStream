using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Represents a named long-running startabe and stoppable task 
    /// </summary>
    public interface IService
    {
        string Name { get; }
        Task Start(CancellationToken token = default);
        Task Stop();
    }
}

