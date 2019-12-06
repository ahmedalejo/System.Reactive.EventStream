using System;
using System.Collections.Generic;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Represents a type with a single value. This type is often used to denote the successful completion of a void-returning method (C#) or a Sub procedure (Visual Basic).
    /// </summary>
    public interface IDisposableHost : ICollection<IDisposable>, IDisposable
    {
        ICollection<IDisposable> Disposables { get; }
        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
