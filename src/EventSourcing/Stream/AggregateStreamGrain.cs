using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Core;
using EventSourcing.Grains;
using Orleans.Concurrency;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace EventSourcing.Stream
{
    #region events

    public abstract class StreamEvent: Event
    {
    }

    public class Subscribed: StreamEvent
    {
        public override string Type { get; set;} = nameof(Subscribed);
        public long IntKey { get; set; }
    }

    public class UnSubscribed: StreamEvent
    {
        public override string Type { get; set;} = nameof(UnSubscribed);
        public long IntKey { get; set; }
    }

    public class UpdatedLastNotifiedEventId: StreamEvent
    {
        public override string Type { get; set;} = nameof(UpdatedLastNotifiedEventId);
        public long LastNotifiedEventId { get; set; }
    }

    #endregion

    #region  State
    public class AggregateStreamState
    { 
        public long? LastNotifiedEventId { get; set; }
    }
    #endregion

    #region Aggregate

    public class AggregateStream : IAggregate<AggregateStreamState, StreamEvent>
    {
        public AggregateStreamState State { get; set; }
        public void Apply(StreamEvent @event)
        {
            switch (@event)
            {
                case UpdatedLastNotifiedEventId updatedLastSentEventId:
                    State.LastNotifiedEventId = updatedLastSentEventId.LastNotifiedEventId;
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    public interface IAggregateStreamReceiver : IGrain
    {
        Task Receive(EventSourcing.Persistance.AggregateEvent @event);
    }

    public interface IAggregateStream: IGrainWithStringKey
    {
        Task Subscribe(IGrainIdentity grainIdentity);
        Task UnSubscribe(IGrainIdentity grainIdentity);
        Task Ping();
    }

    public interface IAggregateStreamSettings
    {
        string AggregateName {get;}
        Dictionary<string, Func<EventSourcing.Persistance.AggregateEvent, IGrainFactory, IAggregateStreamReceiver>> EventReceiverGrainResolverMap { get; }
    }

    public class AggregateStreamSettings : IAggregateStreamSettings
    {
        public string AggregateName { get; private set;}

        public Dictionary<string, Func<Persistance.AggregateEvent, IGrainFactory, IAggregateStreamReceiver>> EventReceiverGrainResolverMap { get; private set; }

        public AggregateStreamSettings(string aggregateName)
        {
            AggregateName = aggregateName;
            EventReceiverGrainResolverMap = new Dictionary<string, Func<Persistance.AggregateEvent, IGrainFactory, IAggregateStreamReceiver>>();
        }
    }

    public static class ConfigureAggregateStreamExtensions
    {
        public static ISiloBuilder ConfigureAggregateStream(this ISiloBuilder builder, string aggregateName, Action<IAggregateStreamSettings> configureAggregateStream)
        {
            var aggregateStreamSettings = new AggregateStreamSettings(aggregateName);

            configureAggregateStream.Invoke(aggregateStreamSettings);

            builder.ConfigureServices((hostBuilder, serviceCollection) => 
            {
                serviceCollection.AddSingleton<IAggregateStreamSettings>(aggregateStreamSettings);
            });
            
            return builder;
        }
    }

    [Reentrant]
    public class AggregateStreamGrain : EventSourceGrain<AggregateStreamState, StreamEvent>, IAggregateStream
    {
        private Queue<EventSourcing.Persistance.AggregateEvent> _eventQueue = new Queue<EventSourcing.Persistance.AggregateEvent>();
        private IAggregateStreamSettings _aggregateStreamSettings;
        
        private string _aggregateName;

        public AggregateStreamGrain(): base("stream", new AggregateStream())
        {
        }
        
        public override async Task OnActivateAsync()
        {
            // set current grian key as aggregateName
            _aggregateName = GrainReference.GetPrimaryKeyString();

            // get IAggregateStreamSettings instance from service collection filtered by grainKey
            _aggregateStreamSettings = ServiceProvider.GetServices<IAggregateStreamSettings>().FirstOrDefault(g => g.AggregateName == _aggregateName);
            
            // throw if not able to get StreamSettings
            if(_aggregateStreamSettings == null)
            {
                throw new InvalidOperationException($"unable to retrieve IAggregateStreamSettings for aggregateName {_aggregateName}");
            }

            // call base OnActivateAsync
            await base.OnActivateAsync();

            // set LastNotifiedEventVersion if needed
            if(!State.LastNotifiedEventId.HasValue)
            {
                // get last event from db
                var lastEvent = await Repository.GetLastAggregateEvent(_aggregateName);

                // get LastNotifiedEventVersion as latest aggregate version from db or 0
                await ApplyEvent(new UpdatedLastNotifiedEventId{ LastNotifiedEventId = lastEvent?.Id ?? 0 });
            }

            // register pollForEvents method
            this.RegisterTimer(PollForEvents, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        private async Task PollForEvents(object args)
        {
            if (State.LastNotifiedEventId.HasValue)
            {
                // get new events
                var unprocessedEvents = await Repository.GetAggregateEvents(_aggregateName, State.LastNotifiedEventId.Value);

                // short circuit if no events to process
                if (unprocessedEvents.Length == 0)
                {
                    return;
                }
                
                // add new events to queue
                foreach (var ev in unprocessedEvents)
                {
                    _eventQueue.Enqueue(ev);
                }

                // trigger notification
                NotifySubscribers().Ignore();
            }
        }

        public async Task NotifySubscribers()
        {
            // dequeue each event and sent to subscribers
            while(_eventQueue.Count > 0)
            {
                var @event = _eventQueue.Peek();
                // iterate thru each eventReceiver resolvers and notify grain
                foreach (var eventReceiverGrainResolver in _aggregateStreamSettings.EventReceiverGrainResolverMap)
                {
                    // get subscriber grain
                    var subscriberGrain = eventReceiverGrainResolver.Value.Invoke(@event, GrainFactory);
                    // send event
                    await subscriberGrain.Receive(@event);
                }

                // update state with LastNotifiedEventVersion
                await ApplyEvent(new UpdatedLastNotifiedEventId{ LastNotifiedEventId = @event.Id });

                // remove from queue
                _eventQueue.Dequeue();
            }
        }

        public async Task Subscribe(IGrainIdentity grainIdentity)
        {
            await this.ApplyEvent(new Subscribed{ IntKey = grainIdentity.PrimaryKeyLong });
        }

        public async Task UnSubscribe(IGrainIdentity grainIdentity)
        {
            await this.ApplyEvent(new UnSubscribed{ IntKey = grainIdentity.PrimaryKeyLong });
        }
    }

    // public static class AggregateStreamConfig
    // {
    //     private static HashSet<string> _streamTypes = new HashSet<string>();
    //     public static void AddType(string type) => _streamTypes.Add(type);
    //     public static IEnumerable<string> StreamTypes = _streamTypes;

    //     // public static ISiloBuilder ConfigureAggregateStream(this ISiloBuilder siloBuilder, Action<AggregateStreamConfig> configure)
    //     // {
    //     //     configure.Invoke(AggregateStreamConfig.AddType);
    //     //     return siloBuilder;
    //     // }
    // }
}