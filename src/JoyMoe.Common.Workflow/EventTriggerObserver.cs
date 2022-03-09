using System;
using System.Threading.Tasks;
using Automatonymous;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Workflow;

public class EventTriggerObserver<TI> : EventObserver<TI> where TI : class, IStateful
{
    public virtual Task PreExecute(EventContext<TI> context) {
        return Task.CompletedTask;
    }

    public Task PreExecute<T>(EventContext<TI, T> context) {
        if (context.Data is IEventData data)
        {
            if (string.IsNullOrWhiteSpace(data.Jockey) || data.JockeyId == 0)
            {
                throw new EventExecutionException();
            }

            context.Instance.LastUpdatedById = data.JockeyId;
            context.Instance.LastUpdatedBy   = data.Jockey;
        }
        else
        {
            if (context.Event.Name != nameof(AutomatonymousStateMachine<TI>.Initial))
            {
                throw new EventExecutionException();
            }
        }

        return Task.CompletedTask;
    }

    public virtual Task PostExecute(EventContext<TI> context) {
        return Task.CompletedTask;
    }

    public virtual Task PostExecute<T>(EventContext<TI, T> context) {
        return Task.CompletedTask;
    }

    public virtual Task ExecuteFault(EventContext<TI> context, Exception exception) {
        return Task.CompletedTask;
    }

    public virtual Task ExecuteFault<T>(EventContext<TI, T> context, Exception exception) {
        return Task.CompletedTask;
    }
}
