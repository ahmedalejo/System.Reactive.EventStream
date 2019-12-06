using System;

namespace System.Reactive.EventStream
{
    public interface IStreamEvent
    {
        Guid ID { get; }
        DateTimeOffset Timestamp { get; }
    }

    public interface IStreamEvent<out TEvent> : IStreamEvent { }
    public interface ISubscriptionOf<out TEvent> : IStreamEvent<TEvent> { }
    public interface IUnsubscriptionOf<out TEvent> : IStreamEvent<TEvent> { }
    public interface IRequestFor<out TEvent> : IStreamEvent<TEvent> { }

    public class RootObservableCreatedFor<TEvent> : StreamEvent<TEvent> { }
    public class RootObservableDestroyedFor<TEvent> : StreamEvent<TEvent> { }
    public class RootObservableCouldNotBeDestroyedFor<TEvent> : StreamEvent<TEvent> { }

    public class RequestFor<TEvent> : StreamEvent<TEvent>, IRequestFor<TEvent> { }
    public class RootSubscriptionOf<TEvent> : StreamEvent<TEvent>, ISubscriptionOf<TEvent> { }
    public class RootUnsubscriptionOf<TEvent> : StreamEvent<TEvent>, IUnsubscriptionOf<TEvent> { }

    public class StreamEvent : IStreamEvent
    {
        public StreamEvent()
        {
            this.Timestamp = DateTimeOffset.Now;
        }
        private Guid? _id;
        public Guid ID => (Guid)(_id ?? (_id = Guid.NewGuid()));
        public DateTimeOffset Timestamp { get; internal set; }

        public override string ToString() => 
            $"{Timestamp:HH:mm:ss fffffff} - {ID}: {this.GetType().Name}";
    }

    public class StreamEvent<T> : StreamEvent, IStreamEvent<T>
    {
        T Payload { get; set; }
        public override string ToString() =>
            $"{Timestamp:HH:mm:ss fffffff} - {ID}: {this.GetType().Name.Replace("´1","")}<{typeof(T).Name}>";
    }
}
