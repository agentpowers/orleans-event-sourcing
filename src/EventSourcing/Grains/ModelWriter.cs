using System.Threading.Tasks;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcing.Grains
{
    public abstract class ModelWriter<TState, TEvent>: Grain
        where TState : class
        where TEvent : AggregateEvent
    {
        public const string GrainPrefix = "writer:";
        // aggregate
        private IAggregate<TState, TEvent> _aggregate;

        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        protected TState State { get { return _aggregate.State ;} }

        // constructor
        protected ModelWriter(IAggregate<TState, TEvent> aggregate)
        {
            _aggregate = aggregate;
        }

        // abstract method to retrieve state and events
        public abstract Task<(TState, TEvent[])> GetCurrentStateAndPendingEvents();

        // abstract method to persist state
        public abstract Task PersistState(TState state);

        // init method
        public async Task Init()
        {
            // get current state
            var (currentState, pendingEvents) = await GetCurrentStateAndPendingEvents();
            
            // set aggregate state
            _aggregate.State = currentState;

            // if there are pending events apply them
            if (pendingEvents != null && pendingEvents.Length > 0)
            {
                // apply each event
                foreach (var dbEvent in pendingEvents)
                {
                    _aggregate.Apply(dbEvent);
                }
                // persist updated state
                await PersistState(_aggregate.State);
            }
        }

        // apply event then persist
        protected async Task ApplyEvent(TEvent @event)
        {
            _aggregate.Apply(@event);
            await PersistState(_aggregate.State);
        }
    }
}