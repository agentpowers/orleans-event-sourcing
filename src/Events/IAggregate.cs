using System;

namespace Events
{
    interface IAggregate<TState, TEvent>
    {       
        TState State {get;}
        /// <summary>
        /// Applies an event to a state
        /// </summary>
        /// <param name="event">event</param>
        /// <returns></returns>
        void Apply(TEvent @event);
    }
}
