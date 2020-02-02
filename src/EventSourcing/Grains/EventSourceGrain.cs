using Orleans;
using System.Threading.Tasks;
using EventSourcing.Persistance;
using Newtonsoft.Json;
using System;

namespace EventSourcing.Grains
{
    internal class EventWrapper
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public Event Event { get; set; }
        
    }
    public abstract class EventSourceGrain<TState, TEvent> : Grain 
        where TState : new()
        where TEvent : Event
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

        protected EventSourceGrain(string aggregateName, IAggregate<TState, TEvent> aggregate)
        {
            _aggregateName = aggregateName;
            _aggregate = aggregate;
        }

        // serialize an event by wrapping that using EventWrapper and then uses JSON.NET typenamehandling
        static string SerializeEvent(TEvent obj)
        {
            return JsonConvert.SerializeObject(new EventWrapper{ Event = obj });
        }

        // deserialize json to event
        static TEvent DeserializeEvent(string json)
        {
            var eventWrapper =  JsonConvert.DeserializeObject<EventWrapper>(json);
            return eventWrapper.Event as TEvent;
        }

        private const string defaultGuidString = "00000000-0000-0000-0100-000000000000";
        /// <summary>
        /// Get grain id string
        /// </summary>
        /// <returns></returns>
        private string GetGrainKey()
        {
            if (this.GetPrimaryKeyLong() > 0)
            {
                return this.GetPrimaryKeyLong().ToString();
            }
            if (this.GetPrimaryKeyString() != null)
            {
                return this.GetPrimaryKeyString();
            }
            var guidKey = this.GetPrimaryKey().ToString();
            if (guidKey != defaultGuidString)
            {
                return guidKey;
            }
            throw new ArgumentException("unable to get primary key");
        }

        /// <summary>
        /// Retrieves current state using snapshot and events
        /// </summary>
        /// <returns></returns>
        public override async Task OnActivateAsync()
        {
            _repository = ServiceProvider.GetService(typeof(IRepository)) as IRepository;
            // generate aggregateType
            var aggregateType = $"{_aggregateName}:{GetGrainKey()}";
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
                // set state as new instance of TState
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
            // TODO: make it configurable when to save snapshot
            if (_eventCount % 10 == 0)
            {
                await SaveSnapshot();
            }
            // update state
            _aggregate.Apply(@event);
        }
    }
}