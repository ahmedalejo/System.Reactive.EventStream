using System;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Provides the implementation for a reactive extensions event stream, 
    /// allowing trending and analysis queries to be performed in real-time 
    /// over the events pushed through the stream. 
    /// </summary>
    public partial interface IEventStream
    {
        /// <summary>
        /// Pushes an event to the stream, causing any 
        /// subscriber to be invoked if appropriate.
        /// </summary>
        void Push<TEvent>(TEvent @event);

        /// <summary>
        /// Observes the events of a given type.
        /// </summary>
        [Obsolete("Use 'OfType' instead")]
        IObservable<TEvent> Of<TEvent>();

        /// <summary>
        /// Observes the events of a given type.
        /// </summary>
        IObservable<TEvent> OfType<TEvent>();
    }
}
