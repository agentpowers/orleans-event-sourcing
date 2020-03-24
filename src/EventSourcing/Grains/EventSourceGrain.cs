using Orleans;
using System.Threading.Tasks;
using EventSourcing.Persistance;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace EventSourcing.Grains
{
    internal class EventWrapper
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IEvent Event { get; set; }
    }


    public abstract class EventSourceGrain<TState, TEvent> : Grain 
        where TState : IState, new()
        where TEvent : IEvent
    {
        // aggregate name
        private readonly string _aggregateName;
        // aggregate
        private IAggregate<TState, TEvent> _aggregate;
        // version number for this instance of aggregate(incremented for each event)
        private long _aggregateVersion = 0;
        
        // db aggregateid
        protected long AggregateId;
        // repository
        protected IRepository Repository {get; private set;}

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

        /// <summary>
        /// Get grain id string
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
            Repository = ServiceProvider.GetService(typeof(IRepository)) as IRepository;
            // get grain key
            var grainKey = GetGrainKey();
            // generate aggregateType
            var aggregateType = $"{_aggregateName}:{grainKey}";
            // get aggregate from db
            var aggregate = await Repository.GetAggregateByTypeName(aggregateType);
            if (aggregate == null)
            {
                // add new aggregate if it doesn't exist
                AggregateId = await Repository.SaveAggregate(_aggregateName, new Aggregate{ Type = aggregateType, Created = DateTime.UtcNow });
                // set state as new instance of TState
                _aggregate.State = new TState();
                // init state with grain id
                _aggregate.State.Init(grainKey);
            }
            else
            {
                // use aggregate id from db
                AggregateId = aggregate.AggregateId;
                // get snapshot and events
                var (snapshot, events) = await Repository.GetSnapshotAndEvents(_aggregateName, AggregateId);
                // apply snapshot if any
                if(snapshot != null)
                {
                    // store aggregate version number
                    _aggregateVersion = snapshot.AggregateVersion;
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
                    var @event = JsonSerializer.DeserializeEvent<TEvent>(dbEvent.Data);
                    _aggregate.Apply(@event);
                    // store aggregate version number
                    _aggregateVersion = dbEvent.AggregateVersion;
                }
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
            await Repository.SaveSnapshot(_aggregateName, new Snapshot{ AggregateId = AggregateId, AggregateVersion = _aggregateVersion, Data = JsonConvert.SerializeObject(_aggregate.State), Created = DateTime.UtcNow });
        }

        /// <summary>
        /// save event to db then apply event to state
        /// snapshot will be created for every 10 events
        /// </summary>
        protected async Task ApplyEvent(TEvent @event)
        {
            // serialize event for db
            var serialized = JsonSerializer.SerializeEvent(@event);
            // increment aggregate version 
            _aggregateVersion++;
            // save event to db
            await Repository.SaveEvent(_aggregateName, new Persistance.Event { AggregateId = AggregateId, AggregateVersion = _aggregateVersion, Type = @event.Type, Data = serialized, Created = DateTime.UtcNow });
            // update state
            _aggregate.Apply(@event);
            // save snapshot if ShouldSaveSnapshot returns true
            if (ShouldSaveSnapshot(@event, _aggregateVersion))
            {
                await SaveSnapshot();
            }
        }

        public virtual bool ShouldSaveSnapshot(TEvent @event, long aggregateVersion)
        {
            return aggregateVersion % 20 == 0;
        }
    }
}