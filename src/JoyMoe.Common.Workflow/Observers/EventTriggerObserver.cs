using System;
using System.Threading.Tasks;
using Automatonymous;
using JoyMoe.Common.Workflow.Models;

namespace JoyMoe.Common.Workflow.Observers
{
    public class EventTriggerObserver<TI> : EventObserver<TI> where TI : class, IStateful
    {
        public Task PreExecute(EventContext<TI> context)
        {
            return Task.CompletedTask;
        }

        public Task PreExecute<T>(EventContext<TI, T> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Data is IEventData data)
            {
                if (string.IsNullOrWhiteSpace(data.Jockey) || data.JockeyId == 0)
                {
                    throw new EventExecutionException();
                }

                context.Instance.Note = data.Note;
                context.Instance.LastUpdatedById = data.JockeyId;
                context.Instance.LastUpdatedBy = data.Jockey;
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

        public Task PostExecute(EventContext<TI> context)
        {
            return Task.CompletedTask;
        }

        public Task PostExecute<T>(EventContext<TI, T> context)
        {
            return Task.CompletedTask;
        }

        public Task ExecuteFault(EventContext<TI> context, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task ExecuteFault<T>(EventContext<TI, T> context, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
