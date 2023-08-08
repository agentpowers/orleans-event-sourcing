using System;
using System.Text.Json;
using System.Threading.Tasks;
using EventSourcing.Persistance;

namespace EventSourcing
{
    public interface IEventSource<TState, TEvent>
        where TState : IState, new()
        where TEvent : IEvent
    {
        /// <summary>
        /// Init with aggregate id string
        /// </summary>
        /// <param name="aggregateName"></param>
        /// <param name="aggregate"></param>
        /// <param name="shouldSaveSnapshot"></param>
        /// <param name="aggregateIdString"></param>
        /// <param name="shouldThrowIfAggregateDoesNotExist"></param>
        /// <returns></returns>
        Task Init(string aggregateName, IAggregate<TState, TEvent> aggregate, Func<TEvent, long, bool> shouldSaveSnapshot, string aggregateIdString, bool shouldThrowIfAggregateDoesNotExist);
        /// <summary>
        /// Get State
        /// </summary>
        /// <value></value>
        TState State { get; }
        /// <summary>
        /// Restore state from snapshot and or events
        /// </summary>
        /// <returns></returns>
        Task Restore();
        /// <summary>
        /// Save snapshot
        /// </summary>
        /// <returns></returns>
        Task SaveSnapshot();
        /// <summary>
        /// Apply(Save) event
        /// </summary>
        Task<long> ApplyEvent(TEvent @event, long? rootEventId = null, long? parentEventId = null);
        /// <summary>
        /// Apply(Save) events
        /// </summary>
        Task<long> ApplyEvents(TEvent[] events, long? rootEventId = null, long? parentEventId = null);
        /// <summary>
        /// Get Aggregate events that occurred after eventId
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<AggregateEvent[]> GetAggregateEvents(long eventId = 0);
        /// <summary>
        /// Get last event
        /// </summary>
        /// <returns></returns>
        Task<AggregateEvent> GetLastAggregateEvent();
        /// <summary>
        /// Get Aggregate events that occurred after eventId
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long eventId = 0);
        /// <summary>
        /// Get last event
        /// </summary>
        /// <returns></returns>
        Task<AggregateEvent> GetLastAggregateEvent(string aggregateName);
    }

    public class EventSource<TState, TEvent> : IEventSource<TState, TEvent>
        where TState : IState, new()
        where TEvent : IEvent
    {
        // aggregate name
        private string _aggregateName;
        // aggregate
        private IAggregate<TState, TEvent> _aggregate;
        // version number for this instance of aggregate(incremented for each event)
        private long _aggregateVersion = 0;
        // db aggregate id
        private long AggregateId;
        // aggregate id string
        private string _aggregateIdString;
        // repository
        private readonly IRepository _repository;
        // call back to figure out if a snapshot should be save for an event
        private Func<TEvent, long, bool> _shouldSaveSnapshot;
        // flag indicating if db aggregate was created in this instance
        private bool _isNewDbAggregate;

        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        public TState State => _aggregate.State;

        public EventSource(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="aggregateName"></param>
        /// <param name="aggregate"></param>
        /// <param name="shouldSaveSnapshot"></param>
        /// <param name="aggregateIdString"></param>
        /// <returns></returns>
        public async Task Init(
            string aggregateName,
            IAggregate<TState, TEvent> aggregate,
            Func<TEvent, long, bool> shouldSaveSnapshot,
            string aggregateIdString,
            bool shouldThrowIfAggregateDoesNotExist)
        {
            // TODO: either validate aggregate name so that it can be prefixed as table name 
            // or convert to valid table name prefix and handle mapping between converted and aggregateName argument
            _aggregateName = aggregateName;
            _aggregate = aggregate;
            _shouldSaveSnapshot = shouldSaveSnapshot;
            _aggregateIdString = aggregateIdString;

            // generate aggregateType
            var aggregateType = $"{_aggregateName}:{_aggregateIdString}";
            // get aggregate from db
            var dbAggregate = await _repository.GetAggregateByTypeName(aggregateType);
            if (dbAggregate == null)
            {
                // throw if settings
                if (shouldThrowIfAggregateDoesNotExist)
                {
                    throw new AggregateDoesNotExistException(_aggregateName);
                }
                // add new aggregate if it doesn't exist
                AggregateId = await _repository.SaveAggregate(
                    new Aggregate
                    {
                        Type = aggregateType,
                        Created = DateTime.UtcNow
                    }
                );
                // flag that aggregate was just created 
                _isNewDbAggregate = true;
            }
            else
            {
                // use aggregate id from db
                AggregateId = dbAggregate.AggregateId;
            }

            // set state as new instance of TState
            _aggregate.State = new TState();
            // init state with grain id
            _aggregate.State.Init(_aggregateIdString);
        }

        /// <summary>
        /// Retrieves current state using snapshot and events
        /// </summary>
        /// <returns></returns>
        public async Task Restore()
        {
            // short-circuit if aggregate was created in this instance(there won't be any snapshots or events in db)
            if (_isNewDbAggregate)
            {
                return;
            }

            // get snapshot and events
            var (snapshot, events) = await _repository.GetSnapshotAndEvents(
                _aggregateName,
                AggregateId
            );
            // apply snapshot if any
            if (snapshot != null)
            {
                // store aggregate version number
                _aggregateVersion = snapshot.AggregateVersion;
                // set snapshot as state
                _aggregate.State = JsonSerializer.Deserialize<TState>(snapshot.Data);
            }
            // apply events
            foreach (var dbEvent in events)
            {
                var @event = (TEvent)EventSerializer.DeserializeEvent(dbEvent);
                _aggregate.Apply(@event);
                // store aggregate version number
                _aggregateVersion = dbEvent.AggregateVersion;
            }
        }

        /// <summary>
        /// save snapshot to database
        /// </summary>
        /// <returns></returns>
        public async Task SaveSnapshot()
        {
            await _repository.SaveSnapshot(
                _aggregateName,
                new Snapshot
                {
                    AggregateId = AggregateId,
                    AggregateVersion = _aggregateVersion,
                    Data = JsonSerializer.Serialize(_aggregate.State),
                    Created = DateTime.UtcNow
                }
            );
        }

        /// <summary>
        /// save event to db then apply event to state
        /// snapshot will be created for every 10 events
        /// Returns id of the newly applied event(from db)
        /// </summary>
        public async Task<long> ApplyEvent(TEvent @event, long? rootEventId = null, long? parentEventId = null)
        {
            // get event type
            var type = @event.GetType();
            // get event type name and version
            var eventIdentity = EventTypeHelper.GetEventIdentity(type);
            // serialize event for db
            var serialized = JsonSerializer.Serialize(@event, type);
            // increment aggregate version 
            _aggregateVersion++;
            // save event to db
            var eventId = await _repository.SaveEvent(
                _aggregateName,
                new Persistance.AggregateEventBase
                {
                    AggregateId = AggregateId,
                    AggregateVersion = _aggregateVersion,
                    Type = eventIdentity.Name,
                    EventVersion = eventIdentity.Version,
                    Data = serialized,
                    RootEventId = rootEventId,
                    ParentEventId = parentEventId,
                    Created = DateTime.UtcNow
                }
            );
            // update state
            _aggregate.Apply(@event);
            // save snapshot if ShouldSaveSnapshot returns true
            if (_shouldSaveSnapshot(@event, _aggregateVersion))
            {
                await SaveSnapshot();
            }

            // return id
            return eventId;
        }

        /// <summary>
        /// save events to db then apply event to state
        /// snapshot will be created for every 10 events
        /// Returns id of the newly applied event(from db)
        /// </summary>
        public async Task<long> ApplyEvents(TEvent[] events, long? rootEventId = null, long? parentEventId = null)
        {
            var aggregateEvents = new AggregateEventBase[events.Length];
            for (var i = 0; i < events.Length; i++)
            {
                // get event type
                var type = events[i].GetType();
                // get event type name and version
                var eventIdentity = EventTypeHelper.GetEventIdentity(type);
                // serialize event for db
                var serialized = JsonSerializer.Serialize(events[i], type);
                // increment aggregate version 
                _aggregateVersion++;
                // add aggregateeventbase
                aggregateEvents[i] = new AggregateEventBase
                {
                    AggregateId = AggregateId,
                    AggregateVersion = _aggregateVersion,
                    Type = eventIdentity.Name,
                    EventVersion = eventIdentity.Version,
                    Data = serialized,
                    RootEventId = rootEventId,
                    ParentEventId = parentEventId,
                    Created = DateTime.UtcNow
                };
            }

            var eventId = await _repository.SaveEvents(
                _aggregateName,
                aggregateEvents
            );

            for (var i = 0; i < events.Length; i++)
            {
                // update state
                _aggregate.Apply(events[i]);

                // save snapshot if ShouldSaveSnapshot returns true
                if (_shouldSaveSnapshot(events[i], _aggregateVersion))
                {
                    await SaveSnapshot();
                }
            }

            // return id
            return eventId;
        }

        public Task<AggregateEvent[]> GetAggregateEvents(long eventId = 0) =>
             _repository.GetAggregateEvents(_aggregateName, eventId);

        public Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long eventId = 0) =>
             _repository.GetAggregateEvents(aggregateName, eventId);

        public Task<AggregateEvent> GetLastAggregateEvent() =>
            _repository.GetLastAggregateEvent(_aggregateName);

        public Task<AggregateEvent> GetLastAggregateEvent(string aggregateName) =>
            _repository.GetLastAggregateEvent(aggregateName);
    }
}
