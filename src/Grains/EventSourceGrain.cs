using Orleans;
using System.Threading.Tasks;
using Events;
using Persistance;
using Newtonsoft.Json;

namespace Grains
{
    internal class EventWrapper
    {
        public Events.Event Event { get; set; }
        
    }
    public abstract class EventSourceGrain<TState, TEvent> : Grain 
        where TState :  new()
        where TEvent : Events.Event
    {
        // aggregate name
        private readonly string _aggregateName;
        // aggregate
        private IAggregate<TState, TEvent> _aggregate;
        // db aggregateid
        private long _aggregateId;
        // repository
        private IRepository _repository;
        // event counter
        private int _eventCount = 0;
        // last event sequence that was written to db
        private long _lastEventSequence = 0;

        /// <summary>
        /// Get current state
        /// </summary>
        /// <value></value>
        protected TState State { get { return _aggregate.State ;} }
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        protected EventSourcingGrain(string aggregateName, IAggregate<TState, TEvent> aggregate)
        {
            _aggregateName = aggregateName;
            _aggregate = aggregate;
        }

        // serialize an event by wrapping that using EventWrapper and then uses JSON.NET typenamehandling
        static string SerializeEvent(TEvent obj)
        {
            return JsonConvert.SerializeObject(new EventWrapper{ Event = obj }, Formatting.Indented, serializerSettings);
        }

        // deserialize json to event
        static TEvent DeserializeEvent(string json)
        {
            var eventWrapper =  JsonConvert.DeserializeObject<EventWrapper>(json, serializerSettings);
            return eventWrapper.Event as TEvent;
        }

        /// <summary>
        /// Retrieves current state using snapshot and events
        /// </summary>
        /// <returns></returns>
        public override async Task OnActivateAsync()
        {
            _repository = ServiceProvider.GetService(typeof(IRepository)) as IRepository;
            // generate aggregateType
            var aggregateType = $"{_aggregateName}:{IdentityString}";
            // get aggregate from db
            var aggregate = await _repository.GetAggregateByTypeName(aggregateType);
            if (aggregate == null)
            {
                // add new aggregate if it doesn't exist
                _aggregateId = await _repository.SaveAggregate(new Aggregate{ Type = aggregateType });
            }
            else
            {
                // use aggregate id from db
                _aggregateId = aggregate.AggregateId;
            }
            // get snapshot and events
            var (snapshot, events) = await _repository.GetSnapshotAndEvents(_aggregateId);
            // apply snapshot if any
            if(snapshot != null)
            {
                // set last sequence id
                _lastEventSequence = snapshot.LastEventSequence;
                // set snapshot as state
                _aggregate.State = JsonConvert.DeserializeObject<TState>(snapshot.Data);
            }
            else
            {
                // set set as new instance of TState
                _aggregate.State = new TState();
            }
            // apply events
            foreach (var dbEvent in events)
            {
                var @event = DeserializeEvent(dbEvent.Data);
                _aggregate.Apply(@event);
            }
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        /// <summary>
        /// save snapshot to database
        /// </summary>
        /// <returns></returns>
        private async Task SaveSnapshot()
        {
            await _repository.SaveSnapshot(new Snapshot{ AggregateId = _aggregateId, LastEventSequence = _lastEventSequence, Data = JsonConvert.SerializeObject(_aggregate.State) });
        }

        /// <summary>
        /// save event to db then apply event to state
        /// snapshot will be created for every 10 events
        /// </summary>
        protected async Task ApplyEvent(TEvent @event)
        {
            // serialize event for db
            var serialized = SerializeEvent(@event);
            // save event to db
            _lastEventSequence = await _repository.SaveEvent(new Persistance.Event { AggregateId = _aggregateId, Type = @event.Type, Data = serialized });
            // increment event count
            _eventCount++;
            // save snapshot when needed(every 10 events)
            if (_eventCount % 10 == 0)
            {
                await SaveSnapshot();
            }
            // update state
            _aggregate.Apply(@event);
        }
    }
}