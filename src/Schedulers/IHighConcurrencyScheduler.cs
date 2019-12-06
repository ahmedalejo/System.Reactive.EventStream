using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Represents a <see cref="IScheduler"/> with optimal concurrency
    /// </summary>
    public interface IHighConcurrencyScheduler : IScheduler
    {
    }
}
