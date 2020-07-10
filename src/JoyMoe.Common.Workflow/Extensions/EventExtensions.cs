using System;

namespace Automatonymous
{
    public static class EventExtensions
    {
        /// <summary>
        /// Get Event TData
        /// </summary>
        /// <param name="event">The event to raise</param>
        public static Type GetEventDataType<TData>(this Event<TData> @event)
        {
            return typeof(TData);
        }
    }
}
