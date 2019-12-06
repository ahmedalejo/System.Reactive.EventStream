using System;

namespace System.Reactive.EventStream
{
    public interface IStringEvent : IStreamEvent
    {
        string Name { get; set; }
    }

    public interface IStringEvent<out TEventPayload> : IStreamEvent<TEventPayload>, IStringEvent
    {
        TEventPayload Payload { get; }
    }

    public class StringEvent<T> : StringEvent, IStringEvent<T>
    {
        public T Payload { get; set; }
    }

    public class StringEvent : StreamEvent
    {
        public string Name { get; set; }
    }
}
