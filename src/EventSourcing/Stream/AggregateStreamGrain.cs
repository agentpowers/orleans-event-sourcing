using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Core;
using EventSourcing.Grains;
using Orleans.Concurrency;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
        public long LastNotifiedEventVersion { get; set; }
    }

    #endregion

    #region  State
    public class AggregateStreamState
    { 
        public HashSet<long> SubscribedGrainIntKeys { get; } = new HashSet<long>();
        public long? LastNotifiedEventVersion { get; set; }
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
                case Subscribed subscribed: 
                    State.SubscribedGrainIntKeys.Add(subscribed.IntKey);
                    break;
                case UnSubscribed unSubscribed:
                    State.SubscribedGrainIntKeys.Remove(unSubscribed.IntKey);
                    break;
                case UpdatedLastNotifiedEventId updatedLastSentEventId:
                    State.LastNotifiedEventVersion = updatedLastSentEventId.LastNotifiedEventVersion;
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    public interface IAggregateStreamReceiverWithIntegerKey : IGrainWithIntegerKey
    {
        Task Receive(EventSourcing.Persistance.Event @event);
    }

    public interface IAggregateStreamReceiver : IGrain
    {
        Task Receive(EventSourcing.Persistance.Event @event);
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
        Dictionary<string, Func<EventSourcing.Persistance.Event, IGrainFactory, IAggregateStreamReceiver>> EventReceiverResolverMap { get; }
    }

    public class AggregateStreamSettings : IAggregateStreamSettings
    {
        public string AggregateName { get; private set;}

        public Dictionary<string, Func<Persistance.Event, IGrainFactory, IAggregateStreamReceiver>> EventReceiverResolverMap { get; private set; }

        public AggregateStreamSettings(string aggregateName)
        {
            AggregateName = aggregateName;
            EventReceiverResolverMap = new Dictionary<string, Func<Persistance.Event, IGrainFactory, IAggregateStreamReceiver>>();
        }
    }

    public static class ConfigureAggregateStreamExtensions
    {
        
        public static ISiloHostBuilder ConfigureAggregateStream(this ISiloHostBuilder siloHostBuilder, string aggregateName, Action<IAggregateStreamSettings> configureAggregateStream)
        {
            var aggregateStreamSettings = new AggregateStreamSettings(aggregateName);

            configureAggregateStream.Invoke(aggregateStreamSettings);

            siloHostBuilder.ConfigureServices((hostBuilder, serviceCollection) => 
            {
                serviceCollection.AddSingleton<IAggregateStreamSettings>(aggregateStreamSettings);
            });
            
            return siloHostBuilder;
        }
    }

    [Reentrant]
    public class AggregateStreamGrain : EventSourceGrain<AggregateStreamState, StreamEvent>, IAggregateStream
    {
        private Queue<EventSourcing.Persistance.Event> _eventQueue = new Queue<EventSourcing.Persistance.Event>();
        const string aggregateName = "stream";

        public AggregateStreamGrain(): base(aggregateName, new AggregateStream())
        {
        }
        
        public override async Task OnActivateAsync()
        {
            // register pollForEvents method
            this.RegisterTimer(PollForEvents, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        private async Task PollForEvents(object args)
        {
            if (State.LastNotifiedEventVersion.HasValue)
            {
                // get new events
                var unprocessedEvents = await Repository.GetEvents(aggregateName, AggregateId, State.LastNotifiedEventVersion.Value);
                
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
                // iterate thru each IntegerKey grain and notify subscribers
                foreach (var grainKey in State.SubscribedGrainIntKeys)
                {
                    // get subscriber grain
                    var subscriberGrain = GrainFactory.GetGrain<IAggregateStreamReceiverWithIntegerKey>(grainKey);
                    // send event
                    await subscriberGrain.Receive(_eventQueue.Peek());
                }

                // update state with LastNotifiedEventVersion
                await ApplyEvent(new UpdatedLastNotifiedEventId{ LastNotifiedEventVersion = _eventQueue.Peek().EventVersion });

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

    public static class AggregateStreamConfig
    {
        private static HashSet<string> _streamTypes = new HashSet<string>();
        public static void AddType(string type) => _streamTypes.Add(type);
        public static IEnumerable<string> StreamTypes = _streamTypes;

        // public static ISiloBuilder ConfigureAggregateStream(this ISiloBuilder siloBuilder, Action<AggregateStreamConfig> configure)
        // {
        //     configure.Invoke(AggregateStreamConfig.AddType);
        //     return siloBuilder;
        // }
    }
}