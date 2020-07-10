using System;
using Automatonymous;
using JoyMoe.Common.Workflow.Models;
using JoyMoe.Common.Workflow.Observers;

namespace JoyMoe.Common.Workflow
{
    public abstract class StateMachineBase<TI> : AutomatonymousStateMachine<TI>, IDisposable where TI : class, IStateful
    {
        private bool _disposed = false;

        private readonly IDisposable? _eventObserver;
        private readonly IDisposable? _stateObserver;

        protected StateMachineBase()
        {
        }

        protected StateMachineBase(StateObserver<TI> observer)
        {
            _eventObserver = this.ConnectEventObserver(new EventTriggerObserver<TI>());
            _stateObserver = this.ConnectStateObserver(observer);
        }

        public State<TI> GetCurrentState(TI instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return GetState(instance.State);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventObserver?.Dispose();
                _stateObserver?.Dispose();
            }

            _disposed = true;
        }
    }
}
