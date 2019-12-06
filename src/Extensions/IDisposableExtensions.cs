using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive.EventStream
{
    public static class IDisposableExtensions
    {
        public static IDisposalContinuation DisposeWith<T>(
            this T disposable,
            IDisposableHost host, Action callback = default)
            where T : IDisposable
        {
            return DisposeWith(
                disposable: disposable, 
                disposables: host.Disposables, 
                callback: callback);
        }

        public static IDisposalContinuation DisposeSafelyWith<T>(
            this T disposable,
            IDisposableHost host, Action callback = default)
            where T : IDisposable
        {
            return DisposeSafelyWith(
                disposable: disposable,
                disposables: host.Disposables,
                callback: callback);
        }

        public static IDisposalContinuation DisposeWith(
            this IDisposable disposable,
            ICollection<IDisposable> disposables, Action callback = default)
        {
            disposables.Add(disposable);
            var list = new List<IDisposable>(2) { disposable };

            if (callback != null)
            {
                disposable = Disposable.Create(callback);
                disposables.Add(disposable);
                list.Add(disposable);
            }

            return Disposable.Create(() =>
            {
                for (int i = 0; i < list.Count; i++)
                    disposables.Remove(list[i]);
            })
            .AsContinuable(disposable);
        }

        public static IDisposalContinuation DisposeSafelyWith<T>(
            this T disposable,
            ICollection<IDisposable> disposables, Action callback = default)
            where T : IDisposable
        {
            var safeDisposable = disposable.MakeSafe();

            disposables.Add(safeDisposable);
            var list = new List<IDisposable>(2) { safeDisposable };

            if (callback != null)
            {
                safeDisposable = Disposable.Create(callback).MakeSafe();
                disposables.Add(safeDisposable);
                list.Add(safeDisposable);
            }

            return Disposable.Create(() =>
            {
                for (int i = 0; i < list.Count; i++)
                    disposables.Remove(list[i]);
            }).MakeSafe()
            .AsContinuable(disposable);
        }

        public static IDisposalContinuation DisposeWith<T>(
            this T disposable,
            CancellationToken cancellationToken)
            where T : IDisposable
        {
            return cancellationToken.Register(disposable.Dispose)
                .AsContinuable(disposable);
        }

        public static IDisposalContinuation DisposeSafelyWith(
            this IDisposable disposable,
            CancellationToken cancellationToken)
        {
            var safeDispose = disposable.MakeSafe();

            return DisposeWith(safeDispose, cancellationToken);
        }

        public static void DisposeSafely(
            this IDisposable disposable)
        {
            disposable.MakeSafe().Dispose();
        }

        public static void Dispose(
            this ICollection<IDisposable> disposables)
        {
            var litter = disposables is CompositeDisposable composite
                ? composite
                : new CompositeDisposable(disposables);

            litter?.Dispose();
        }

        public static IDisposalContinuation DisposeSafelyOnCompleted<TEvent>(
            this IDisposable disposable,
            IObservable<TEvent> observable)
        {
            var safeDispose = disposable.MakeSafe();

            return DisposeOnCompleted(safeDispose, observable);
        }

        public static IDisposalContinuation DisposeOnCompleted<TEvent>(
            this IDisposable disposable,
            IObservable<TEvent> observable)
        {
            return observable.LastAsync().Subscribe(
                onNext: _ => { },
                onError: _ => disposable.Dispose(),
                onCompleted: disposable.Dispose)
                .AsContinuable(disposable);
        }

        public static IDisposalContinuation DisposeSafelyOnNext<TEvent>(
            this IDisposable disposable,
            IObservable<TEvent> observable)
        {
            var safeDispose = disposable.MakeSafe();

            return DisposeOnNext(safeDispose, observable);
        }

        public static IDisposalContinuation DisposeOnNext<TEvent>(
            this IDisposable disposable,
            IObservable<TEvent> observable)
        {
            return observable.Take(1).Subscribe(
                onNext: _ => disposable.Dispose(),
                onError: _ => disposable.Dispose())
                .AsContinuable(disposable);
        }

        public static IDisposalContinuation DisposeAfer(
            this IDisposable disposable,
            TimeSpan duration)
        {
            var cancellationTokenSource = new CancellationTokenSource(duration);
            return disposable.DisposeWith(cancellationTokenSource.Token);
        }

        public static IDisposalContinuation DisposeSafelyAfer(
            this IDisposable disposable,
            TimeSpan duration)
        {
            var safeDispose = disposable.MakeSafe();
            return DisposeAfer(safeDispose, duration);
        }

        private static IDisposalContinuation AsContinuable(
            this IDisposable subscription,
            IDisposable original)
            => new DisposalContinuation(subscription, original);

        public static IDisposable MakeSafe(this IDisposable disposable, string name = null)
        {
            if (disposable == null)
                return Disposable.Empty;

            return Disposable.Create(() =>
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    if (name is null)
                        System.Diagnostics.Debug.Write(ex);
                    else
                        System.Diagnostics.Debug.Write(ex, name);
                }
            });
        }

        private class DisposalContinuation : IDisposalContinuation
        {
            private readonly IDisposable disposable;
            private readonly IDisposable subscription;

            public DisposalContinuation(IDisposable subscription, IDisposable disposable)
            {
                this.disposable = disposable;
                this.subscription = subscription;
            }

            IDisposable IDisposalContinuation.And => this.disposable;

            void IDisposable.Dispose() => this.subscription.Dispose();
        }

        public interface IDisposalContinuation : IDisposable
        {
            IDisposable And { get; }
        }
    }



}
