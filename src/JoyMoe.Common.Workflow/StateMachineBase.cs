using Automatonymous;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Workflow;

public abstract class StateMachineBase<TI> : AutomatonymousStateMachine<TI>, IDisposable where TI : class, IStateful
{
    private bool _disposed;

    private readonly IDisposable? _eventObserver;
    private readonly IDisposable? _stateObserver;

    protected StateMachineBase() { }

    protected StateMachineBase(StateObserver<TI> observer) {
        _stateObserver = this.ConnectStateObserver(observer);
    }

    protected StateMachineBase(EventObserver<TI> eventObserver, StateObserver<TI> stateObserver) {
        _eventObserver = this.ConnectEventObserver(eventObserver);
        _stateObserver = this.ConnectStateObserver(stateObserver);
    }

    public State<TI> GetCurrentState(TI instance) {
        return GetState(instance.State);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposed) return;

        if (disposing) {
            _eventObserver?.Dispose();
            _stateObserver?.Dispose();
        }

        _disposed = true;
    }
}
