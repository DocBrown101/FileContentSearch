namespace ContentSearch
{
    using System;
    using System.Threading;

    public class AppState : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private State currentState;
        private bool isDisposed;

        public AppState()
        {
            this.CurrentState = State.Idle;
        }

        public event EventHandler<State> CurrentStateChanged;

        public enum State
        {
            Idle,
            Running,
            Aborting
        }

        public State CurrentState
        {
            get => this.currentState;

            private set
            {
                this.currentState = value;

                this.TriggerCurrentStateChanged(value);
            }
        }

        public CancellationToken Run()
        {
            this.CurrentState = State.Running;

            this.cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = this.cancellationTokenSource.Token;

            return cancellationToken;
        }

        public void Abort()
        {
            if (this.cancellationTokenSource != null)
            {
                this.CurrentState = State.Aborting;

                this.cancellationTokenSource.Cancel();
            }
        }

        public void Idle() => this.CurrentState = State.Idle;

        private void TriggerCurrentStateChanged(State state) => this.CurrentStateChanged?.Invoke(null, state);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed) return;

            if (disposing)
            {
                this.cancellationTokenSource.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
