namespace EventSourcing
{
    public interface IAggregate<TState, TEvent>
    {       
        TState State {get; set;}
        /// <summary>
        /// Applies an event to a state
        /// </summary>
        /// <param name="event">event</param>
        /// <returns></returns>
        void Apply(TEvent @event);
    }
}
