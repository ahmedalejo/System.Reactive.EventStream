using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive.EventStream
{
    public static class IEventStreamExtensions
    {
        /// <summary>
        /// Pushes a string the given <see cref="IEventStream"/>
        /// with an associated payload
        /// </summary>
        public static void Push<TPayload>(
            this IEventStream eventStream,
            string name,
            TPayload payload) => 
            eventStream.Push(new StringEvent<TPayload>
            {
                Name = name,
                Payload = payload
            });

        /// <summary>
        /// Observes the events of a given name.
        /// </summary>
        private static IObservable<IStringEvent<T>> OfNamedString<T>(
            this IEventStream eventStream,
            string name)
        {
            return eventStream
                .OfType<IStringEvent<T>>()
                .Where(_ => _.Name == name);
        }

        /// <summary>
        /// Observes the events of a given name.
        /// </summary>
        public static IDisposable OfName<T>(
            this IEventStream eventStream,
            string name,
            Action<T> onNext)
        {
            if (onNext == null)
                return Disposable.Empty;

            return eventStream
                .OfNamedString<T>(name)
                .Select(_ => _.Payload)
                .Retry()
                .Subscribe(onNext: onNext);
        }

        /// <summary>
        /// Observes the events of a given name.
        /// </summary>
        public static IObservable<T> OfName<T>(
            this IEventStream eventStream,
            string name)
        {
            return eventStream
                .OfNamedString<T>(name)
                .Retry()
                .Select(_ => _.Payload);
        }

        /// <summary>
        /// Observes the events of a given name.
        /// </summary>
        public static IObservable<Unit> OfName(
            this IEventStream eventStream,
            string name)
        {
            var none = Unit.Default;

            return eventStream
                .OfType<string>()
                .Where(_ => _ == name)
                .Select(_ => none)
                .Merge(eventStream
                .OfType<IStringEvent>()
                .Where(_ => _.Name == name)
                .Select(_ => none));
        }
        /// <summary>
        /// Observes the events of a given name.
        /// </summary>
        public static IDisposable OfName(
            this IEventStream eventStream,
            string name,
            Action onNext)
        {
            if (onNext == null)
                return Disposable.Empty;

            return eventStream
                .OfName(name)
                .Subscribe(_ => onNext());
        }
        /// <summary>
        /// Observes the events of a given type.
        /// </summary>
        public static IDisposable OfType<TEvent>(
            this IEventStream eventStream,
            Action<TEvent> onNext)
        {
            return eventStream.OfType<TEvent>().Subscribe(onNext);
        }

        public static IDisposable RegisterService<TEvent>(
            this IEventStream eventStream,
            IService service)
        {
            var name = string.IsNullOrWhiteSpace(service.Name)
                ? service.Name
                : service.GetType().ToString();

            var cts = new CancellationTokenSource();

            return eventStream.RegisterService<TEvent>(
                name,
                onStart: () => service.Start(cts.Token),
                onStop: () =>
                {
                    cts.Cancel();
                    service.Stop();
                });
        }

        public static IDisposable RegisterService<TEvent>(
            this IEventStream eventStream,
            string name,
            Action onStart,
            Action onStop,
            Action onPrepare = null,
            Action<TEvent> onNext = null)
        {
            return eventStream.RegisterService<TEvent>(
                onStart: onStart,
                onStop: onStop,
                onPrepare: onPrepare,
                onNext: onNext);
        }

        private static IDisposable RegisterService<TEvent>(
            this IEventStream eventStream,
            Action onStart,
            Action onStop,
            Action onPrepare = null,
            Action<TEvent> onNext = null,
            string name = null)
        {
            var disposables = new CompositeDisposable(capacity: 4);

            if (onNext != null)
            {
                eventStream
                    .OfType<TEvent>()
                    .Subscribe(onNext)
                    .DisposeWith(disposables);
            }

            if (onPrepare != null)
            {
                eventStream
                    .OfType<IRequestFor<TEvent>>()
                    .Subscribe(_ => onPrepare())
                    .DisposeWith(disposables);
            }

            if (onStart != null)
            {
                eventStream
                    .OfType<ISubscriptionOf<TEvent>>()
                    .Subscribe(_ =>
                    {
                        onStart();

                        eventStream.Push(new ServiceStartedEvent(name));
                    })
                    .DisposeWith(disposables);
            }

            if (onStop == null)
            {
                eventStream
                    .OfType<IUnsubscriptionOf<TEvent>>()
                    .Subscribe(_ =>
                    {
                        onStop();

                        eventStream.Push(new ServiceStoppedEvent(name));
                    })
                    .DisposeWith(disposables);
            }

            eventStream.Push(new ServiceRegisteredEvent(name));

            return disposables;
        }

    }
}
