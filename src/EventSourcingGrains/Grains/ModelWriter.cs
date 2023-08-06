using System.Threading.Tasks;
using EventSourcing;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcingGrains.Grains
{
    public interface IModelWriterState
    {
        long Version { get; }
    }

    public abstract class ModelWriter<TState, TEvent> : Grain
        where TState : IModelWriterState
        where TEvent : AggregateEvent
    {
        public const string GrainPrefix = "writer:";
        // aggregate
        private readonly IAggregate<TState, TEvent> _aggregate;

        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        protected TState State => _aggregate.State;

        // constructor
        protected ModelWriter(IAggregate<TState, TEvent> aggregate)
        {
            _aggregate = aggregate;
        }

        // abstract method to retrieve state
        public abstract Task<TState> GetCurrentState();
        // abstract method to retrieve events
        public abstract Task<TEvent[]> GetPendingEvents(long currentVersion);

        // abstract method to persist state
        public abstract Task PersistState(TState state);

        // init method
        public async Task Init()
        {
            // get current state
            var currentState = await GetCurrentState();
            // get pending events
            var pendingEvents = await GetPendingEvents(currentState.Version);

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

        public async Task RecoverState()
        {
            // get pending events
            var pendingEvents = await GetPendingEvents(State.Version);

            // if there are pending events apply them
            if (pendingEvents != null && pendingEvents.Length > 0)
            {
                // apply each event
                foreach (var dbEvent in pendingEvents)
                {
                    _aggregate.Apply(dbEvent);
                }
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