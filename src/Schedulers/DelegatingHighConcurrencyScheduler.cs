using System.Reactive.Concurrency;

namespace System.Reactive.EventStream
{
    public class DelegatingHighConcurrencyScheduler 
        : DelegatingScheduler
        , IHighConcurrencyScheduler
    {
        public DelegatingHighConcurrencyScheduler(IScheduler scheduler) 
            : base(scheduler)
        { }
    }
}
