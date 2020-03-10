using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing;
using Orleans.Core;
using EventSourcing.Persistance;
using Event = EventSourcing.Event;

namespace Grains.Test
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

    public class AddedEventId: StreamEvent
    {
        public override string Type { get; set;} = nameof(AddedEventId);
        public long EventId { get; set; }
    }

    public class RemovedEventId: StreamEvent
    {
        public override string Type { get; set;} = nameof(RemovedEventId);
        public long EventId { get; set; }
    }

    #endregion

    #region  State
    public class StreamState
    {
        public HashSet<long> IntKeys { get; } = new HashSet<long>();
        public Queue<long> UnprocessedEventIds { get; } = new Queue<long>();
    }
    #endregion

    #region Aggregate

    public class StreamAggregate : IAggregate<StreamState, StreamEvent>
    {
        public StreamState State { get; set; }
        public void Apply(StreamEvent @event)
        {
            switch (@event)
            {
                case Subscribed subscribed: 
                    State.IntKeys.Add(subscribed.IntKey);
                    break;
                case UnSubscribed unSubscribed:
                    State.IntKeys.Remove(unSubscribed.IntKey);
                    break;
                case AddedEventId addedEventId:
                    State.UnprocessedEventIds.Enqueue(addedEventId.EventId);
                    break;
                case RemovedEventId removedEventId:
                    State.UnprocessedEventIds.Dequeue();
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    public interface IAggregateStreamReceiverWithIntegerKey : IGrainWithIntegerKey
    {
        Task Receive(EventSourcing.Persistance.Event[] events);
    }

    internal interface IAggregateStream
    {
        Task Subscribe(IGrainIdentity grainIdentity);
        Task UnSubscribe(IGrainIdentity grainIdentity);
        Task Ping();
    }

    public class Stream : Grain, IAggregateStream, IGrainWithStringKey
    {
        private List<Event> _events = new List<Event>();
        private HashSet<long> _longKeys = new HashSet<long>();
        private IRepository _repository;
        public override async Task OnActivateAsync()
        {
            _repository = ServiceProvider.GetService(typeof(IRepository)) as IRepository;
            this.RegisterTimer(PollForEvents, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            await base.OnActivateAsync();
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        private Task PollForEvents(object args)
        {
            return Task.CompletedTask;
        }

        public Task Push(Event @event)
        {
            throw new NotImplementedException();
        }

        // public async Task SendMessage(object args)
        // {
        //     var value = $"{this.GetPrimaryKeyString()}--{DateTime.UtcNow}";
        //     foreach (var key in _keys)
        //     {
        //         var receiver = this.GrainFactory.GetGrain<IReceiver>(key);
        //         await receiver.ReceiveMessage($"{key}--{value}");
        //     }
        // }

        // Clients call this to subscribe.
        // public Task Subscribe(string grainKey)
        // {
        //     _keys.Add(grainKey);
        //     return Task.CompletedTask;
        // }

        public Task Subscribe(IGrainIdentity grainIdentity)
        {
            _longKeys.Add(grainIdentity.PrimaryKeyLong);
            return Task.CompletedTask;
        }

        //Also clients use this to unsubscribe themselves to no longer receive the messages.
        // public Task UnSubscribe(string grainKey)
        // {
        //     _keys.Remove(grainKey);
        //     return Task.CompletedTask;
        // }

        public Task UnSubscribe(IGrainIdentity grainIdentity)
        {
            _longKeys.Remove(grainIdentity.PrimaryKeyLong);
            return Task.CompletedTask;
        }

        private async Task SendEvents(EventSourcing.Persistance.Event[] events)
        {
            foreach (var key in _longKeys)
            {
                var receiver = this.GrainFactory.GetGrain<IAggregateStreamReceiverWithIntegerKey>(key);
                await receiver.Receive(new Event[] {});
            }
        }
    }
}