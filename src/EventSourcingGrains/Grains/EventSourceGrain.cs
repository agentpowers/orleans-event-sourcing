using Orleans;
using System.Threading.Tasks;
using System;
using System.Linq;
using EventSourcing;
using EventSourcingGrains.Stream;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingGrains.Grains
{
    public abstract class EventSourceGrain<TState, TEvent> : Grain
        where TState : IState, new()
        where TEvent : IEvent
    {
        protected IEventSource<TState, TEvent> EventSource;
        private EventSourceGrainSetting _eventSourceGrainSettings = null;
        private string _aggregateName;
        private IAggregate<TState, TEvent> _aggregate;
        private bool _hasAggregateStream;
        
        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        protected TState State { get { return EventSource.State ;} }

        protected EventSourceGrain(string aggregateName, IAggregate<TState, TEvent> aggregate)
        {
            _aggregateName = aggregateName;
            _aggregate = aggregate;
        }

        /// <summary>
        /// Get grain id string.  Override this when possible, since this implementation uses reflection
        /// </summary>
        /// <returns></returns>
        public virtual string GetGrainKey()
        {
            var interfaces = this.GetType().GetInterfaces().ToList();
            if (interfaces.Any(x => x.Equals(typeof(IGrainWithIntegerKey))))
            {
                return this.GetPrimaryKeyLong().ToString();
            }
            if (interfaces.Any(x => x.Equals(typeof(IGrainWithStringKey))))
            {
                return this.GetPrimaryKeyString();
            }
            if (interfaces.Any(x => x.Equals(typeof(IGrainWithGuidKey))))
            {
                return this.GetPrimaryKey().ToString();
            }
            throw new InvalidOperationException("unable to retrieve GrainKey");
        }

        /// <summary>
        /// Retrieves current state using snapshot and events
        /// </summary>
        /// <returns></returns>
        public override async Task OnActivateAsync()
        {
            // does this aggregate has stream settings
            _hasAggregateStream = ServiceProvider.GetServices<IAggregateStreamSettings>().Any(g => g.AggregateName == _aggregateName);
            // get grain settings
            var eventSourceGrainSettingsMap = (IEventSourceGrainSettingsMap)ServiceProvider.GetService(typeof(IEventSourceGrainSettingsMap));
            eventSourceGrainSettingsMap.TryGetValue(_aggregateName, out _eventSourceGrainSettings);
            // get instance of EventSource
            EventSource = (IEventSource<TState, TEvent>)ServiceProvider.GetService(typeof(IEventSource<TState, TEvent>));
            // get grain key
            var grainKey = GetGrainKey();
            // init EventSource
            await EventSource.Init(_aggregateName, _aggregate, ShouldSaveSnapshot, grainKey, _eventSourceGrainSettings?.ShouldThrowIfAggregateDoesNotExist ?? false);
            // restore EventSource State
            await EventSource.Restore();
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        /// <summary>
        /// save snapshot to database
        /// </summary>
        /// <returns></returns>
        protected Task SaveSnapshot()
        {
            return EventSource.SaveSnapshot();
        }

        /// <summary>
        /// save event to db then apply event to state
        /// snapshot will be created for every 20 events
        /// Returns id of the newly applied event(from db)
        /// </summary>
        protected async Task<long> ApplyEvent(TEvent @event, long? rootEventId = null, long? parentEventId = null)
        {
            // apply event to EventSource
            var eventId = await EventSource.ApplyEvent(@event, rootEventId, parentEventId);
            // does this aggregate has aggregate stream settings
            if (_hasAggregateStream)
            {
                // notify AggregateStreamGrain about new event
                await GrainFactory.GetGrain<IAggregateStreamGrain>(_aggregateName).Notify(eventId);
            }
            // return eventId
            return eventId;
        }

        public virtual bool ShouldSaveSnapshot(TEvent @event, long aggregateVersion)
        {
            return aggregateVersion % 20 == 0;
        }
    }
}