using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Reactive.EventStream
{
    /// <summary>
    /// Represents a group of disposable resources that are disposed together.
    /// </summary>
    public class TrashCan : IDisposableHost
    {
        readonly CompositeDisposable disposables = new CompositeDisposable();
        #region ICollection<IDisposable>
        public ICollection<IDisposable> Disposables => disposables;
        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed => this.disposables.IsDisposed;
        #endregion IEnumerable



        #region ICollection<IDisposable>
        public void Add(IDisposable item) => this.Disposables.Add(item);
        public int Count => this.Disposables.Count;
        public void Clear() => this.Disposables.Clear();
        public bool Contains(IDisposable item) => this.Disposables.Contains(item);
        public void CopyTo(IDisposable[] array, int arrayIndex) => this.Disposables.CopyTo(array, arrayIndex);
        public bool IsReadOnly => this.Disposables.IsReadOnly;
        public bool Remove(IDisposable item) => this.Disposables.Remove(item);
        #endregion ICollection<IDisposable>

        #region IEnumerable<IDisposable>
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() => this.Disposables.GetEnumerator();
        #endregion IEnumerable<IDisposable>

        #region IEnumerable
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.Disposables.GetEnumerator();
        #endregion IEnumerable

        #region IDisposable
        /// <summary>
        /// Disposes all disposables in the group and removes them from the group.
        /// </summary>
        public void Dispose() => this.disposables.Dispose();
        #endregion IDisposable
    }
}
