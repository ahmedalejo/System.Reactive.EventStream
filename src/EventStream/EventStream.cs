using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Provides the implementation for a reactive extensions event stream, 
    /// allowing trending and analysis queries to be performed in real-time 
    /// over the events pushed through the stream. 
    /// </summary>
    public partial class EventStream : IEventStream
    {
        internal ConcurrentDictionary<Type, object> innerEventStreams = new ConcurrentDictionary<Type, object>();
        ISubject<object> innerEventStream = new Subject<object>();
        private IObservable<object> streamObservable;

        public EventStream(IScheduler scheduler)
        {
            this.Scheduler = scheduler;

            innerEventStream = new Subject<object>();
            this.streamObservable = innerEventStream
                .SubscribeOn(this.Scheduler)
                .Publish()
                .RefCount();

            this.EmitsMetaEvents = true;
        }

        public bool EmitsMetaEvents { get; protected set; }

        public static IEventStream DefaultStream { get; } = new EventStream(TaskPoolScheduler.Default)
        {
            EmitsMetaEvents = true
        };

        public IScheduler Scheduler { get; set; }

        /// <summary>
        /// Pushes an event to the stream, causing any  
        /// subscriber to be invoked if appropriate.
        /// </summary>
        /// <remarks>
        /// This overload will not invoke subscribers for the 
        /// same <typeparamref name="TEvent"/> but as an 
        /// <see cref="IEventPattern{TEvent}"/>, because no 
        /// sender information is provided and therefore 
        /// is not available.
        /// </remarks>
        public void Push<TEvent>(TEvent @event) =>
            this.innerEventStream.OnNext(@event);

        /// <summary>
        /// Observes the events of a given type.
        /// </summary>
        [Obsolete("Use 'OfType' instead")]
        public IObservable<TEvent> Of<TEvent>() => this.OfType<TEvent>();

        /// <summary>
        /// Observes the events of a given type.
        /// </summary>
        public IObservable<TEvent> OfType<TEvent>()
        {
            if (this.EmitsMetaEvents)
            {
                Debug.WriteLine($"RequestFor<{typeof(TEvent)}>");
                this.Push(new RequestFor<TEvent>());
            }

            var observable = innerEventStreams.GetOrAdd(typeof(TEvent), _ => CreateObservable<TEvent>());

            return observable as IObservable<TEvent>;
        }

        IObservable<TEvent> CreateObservable<TEvent>()
        {
            if (this.EmitsMetaEvents)
                this.Push(new RootObservableCreatedFor<TEvent>());

            return Observable.Create<TEvent>(observer =>
            {
                var disposable = streamObservable.OfType<TEvent>().Subscribe(observer);

                if (this.EmitsMetaEvents)
                    this.Push(new RootSubscriptionOf<TEvent>());

                return new CompositeDisposable(disposable, Disposable.Create(() =>
                {
                    if (this.EmitsMetaEvents)
                        this.Push(new RootUnsubscriptionOf<TEvent>());

                    var observableWasRemoved = innerEventStreams.TryRemove(typeof(TEvent), out object _);

                    if (!this.EmitsMetaEvents)
                        return;
                    
                    if (observableWasRemoved)
                    {
                        this.Push(new RootObservableDestroyedFor<TEvent>());
                    }
                    else
                    {
                        this.Push(new RootObservableCouldNotBeDestroyedFor<TEvent>());
                    }                    
                }).MakeSafe());
                
            })
            .SubscribeOn(this.Scheduler)
            .ObserveOn(this.Scheduler)
            .Publish()
            .RefCount();
        }
    }
}
