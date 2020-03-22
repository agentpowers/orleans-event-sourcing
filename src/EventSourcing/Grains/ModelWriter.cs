using System.Threading.Tasks;
using Newtonsoft.Json;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcing.Grains
{
    public abstract class ModelWriter<TState, TEvent>: Grain
        where TState : class
        where TEvent : AggregateEvent
    {
        // aggregate
        private IAggregate<TState, TEvent> _aggregate;

        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        protected TState State { get { return _aggregate.State ;} }

        protected ModelWriter(IAggregate<TState, TEvent> aggregate)
        {
            _aggregate = aggregate;
        }

        public abstract Task<(TState, TEvent[])> GetCurrentStateAndPendingEvents();
        public abstract Task PersistState(TState state);

        public async Task Init()
        {
            // get current state
            var (currentState, pendingEvents) = await GetCurrentStateAndPendingEvents();
            
            _aggregate.State = currentState;
            if (pendingEvents != null && pendingEvents.Length > 0)
            {
                foreach (var dbEvent in pendingEvents)
                {
                    _aggregate.Apply(dbEvent);
                }
                await PersistState(_aggregate.State);
            }
            
        }

        protected async Task ApplyEvent(TEvent @event)
        {
            _aggregate.Apply(@event);
            await PersistState(_aggregate.State);
        }
    }
}