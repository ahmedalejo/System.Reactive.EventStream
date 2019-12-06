using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Reactive.EventStream
{
    public abstract class ServiceRunnerBase : IService, IDisposable
    {
        public bool IsRunning { get; protected set; }

        public ICommand Command { get; protected set; }

        public string Name { get; set; }

        public abstract Task Start(CancellationToken token);
        public abstract Task Stop();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;
            
            if (disposing)
                _ = Stop();

            disposedValue = true;            
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() => Dispose(true);
        #endregion
    }
}

