using System;

namespace Events
{
    /*
    type Aggregate<'TState, 'TCommand, 'TEvent> = {
    
        /// An initial state value.
        zero : 'TState;

        /// Applies an event to a state returning a new state.
        apply : 'TState -> 'TEvent -> 'TState;

        /// Executes a command on a state yielding new state and new event.
        exec : 'TState -> ('TCommand -> ('TState*'TEvent));
    }
    */

    interface IAggregate<TState, TCommand, TEvent>
    {       
        /// <summary>
        /// Applies an event to a state
        /// </summary>
        /// <param name="event">event</param>
        /// <param name="state">state</param>
        /// <returns></returns>
        TState Apply(TEvent @event, TState state);

        /// <summary>
        /// Executes a command on a state yielding events.
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="state">state</param>
        /// <returns></returns>
        (TState, TEvent) Exec(TCommand command, TState state);
    }
}
