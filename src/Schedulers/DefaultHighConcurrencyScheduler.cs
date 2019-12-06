using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Represents an object that schedules units of work on the Task Parallel Library
    /// (TPL) task pool
    /// </summary>
    public class DefaultHighConcurrencyScheduler 
        : DelegatingHighConcurrencyScheduler
        , ISchedulerLongRunning
        , ISchedulerPeriodic
        , IStopwatchProvider
        , IServiceProvider
    {
        public TaskPoolScheduler TaskPool { get; set; }
        public DefaultHighConcurrencyScheduler()
            : base(TaskPoolScheduler.Default)
        { }
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type serviceType. -or- null if there is no service object
        /// of type serviceType
        /// </returns>
        object IServiceProvider.GetService(Type serviceType) 
            => ((IServiceProvider)this.TaskPool).GetService(serviceType);

        /// <summary>
        /// Starts a new stopwatch object.
        /// </summary>
        /// <returns>New stopwatch object; started at the time of the request.</returns>
        IStopwatch IStopwatchProvider.StartStopwatch() 
            => ((IStopwatchProvider)this.TaskPool).StartStopwatch();

        /// <summary>
        /// Schedules a periodic piece of work.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">Initial state passed to the action upon the first iteration.</param>
        /// <param name="period">Period for running the work periodically.</param>
        /// <param name="action">Action to be executed, potentially updating the state.</param>
        /// <returns>The disposable object used to cancel the scheduled recurring action (best effort).</returns>
        IDisposable ISchedulerPeriodic.SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action) 
            => ((ISchedulerPeriodic)this.TaskPool).SchedulePeriodic(state, period, action);

        /// <summary>
        /// Schedules a long-running piece of work.
        /// </summary>
        /// <typeparam name="TState">Type of state passed to the action to be executed.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        IDisposable ISchedulerLongRunning.ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action) 
            => ((ISchedulerLongRunning)this.TaskPool).ScheduleLongRunning(state, action);
    }
}
