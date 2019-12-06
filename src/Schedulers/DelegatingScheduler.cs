using System;
using System.Reactive.Concurrency;

namespace System.Reactive.EventStream
{
    public class DelegatingScheduler : IScheduler
    {
        private readonly IScheduler scheduler;

        public DelegatingScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public DateTimeOffset Now => scheduler.Now;

        public IDisposable Schedule<TState>(
            TState state, 
            Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, action);
        }

        public IDisposable Schedule<TState>(
            TState state, 
            TimeSpan dueTime, 
            Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(
            TState state, 
            DateTimeOffset dueTime, 
            Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }
    }
}
