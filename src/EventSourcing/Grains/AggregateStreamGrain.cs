using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Logging;
using EventSourcing.Stream;

namespace EventSourcing.Grains
{
    [Reentrant]
    public class AggregateStreamGrain : EventSourceGrain<AggregateStreamState, IStreamEvent>, IAggregateStreamGrain
    {
        private Queue<EventSourcing.Persistance.AggregateEvent> _eventQueue = new Queue<EventSourcing.Persistance.AggregateEvent>();
        private IAggregateStreamSettings _aggregateStreamSettings;
        private readonly ILogger<AggregateStreamGrain> _logger;
        private bool _isNotifyingSubscribers = false;
        private long _lastQueuedEventId;
        
        private string _aggregateName;

        public AggregateStreamGrain(ILogger<AggregateStreamGrain> logger): base("stream", new AggregateStream())
        {
            _logger = logger;
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
            if(State.LastNotifiedEventId == 0)
            {
                // init aggregate tables
                await Repository.CreateEventsAndSnapshotsTables(_aggregateName);
                
                // get last event from db
                var lastEvent = await Repository.GetLastAggregateEvent(_aggregateName);

                // get LastNotifiedEventVersion as latest aggregate version from db or 0
                await ApplyEvent(new UpdatedLastNotifiedEventId{ LastNotifiedEventId = lastEvent?.Id ?? 0 });
            }

            // sync up lastQueuedEventId and LastNotifiedEventId
            _lastQueuedEventId = State.LastNotifiedEventId;

            // register pollForEvents method
            this.RegisterTimer(PollForEvents, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Ping method(to keep grain alive)
        /// </summary>
        /// <returns></returns>
        public Task Ping()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method will get events that are greater than last notified eventId from database and 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task PollForEvents(object args)
        {
            // get new events after last Queued Event Id
            var newEvents = await Repository.GetAggregateEvents(_aggregateName, _lastQueuedEventId);

            // short circuit if no events to process
            if (newEvents.Length == 0)
            {
                return;
            }
            
            // add new events to queue
            foreach (var ev in newEvents)
            {
                // only add to queue if new(just in case)
                if (ev.Id > _lastQueuedEventId)
                {
                    _eventQueue.Enqueue(ev);
                    _lastQueuedEventId = ev.Id;
                }
            }

            // trigger notification
            NotifySubscribers().Ignore();
        }

        public async Task NotifySubscribers()
        {
            // if already notifying then return
            if (_isNotifyingSubscribers)
            {
                return;
            }
            // set flag to true
            _isNotifyingSubscribers = true;
            try
            {
                // dequeue each event and sent to subscribers
                while(_eventQueue.Count > 0)
                {
                    var @event = _eventQueue.Peek();
                    // iterate thru each eventReceiver resolvers and notify grain
                    foreach (var eventReceiverGrainResolver in _aggregateStreamSettings.EventReceiverGrainResolverMap)
                    {
                        // TODO: set up auto retry
                        try
                        {
                            // get subscriber grain via resolver (resolver can return null if there is no need to notify)
                            var subscriberGrain = eventReceiverGrainResolver.Value.Invoke(@event, GrainFactory);
                            if (subscriberGrain != null)
                            {
                                // send event
                                await subscriberGrain.Receive(@event);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogError(ex, $"Error NotifySubscriber {eventReceiverGrainResolver.Key}");
                        }
                    }

                    // update state with LastNotifiedEventVersion
                    await ApplyEvent(new UpdatedLastNotifiedEventId{ LastNotifiedEventId = @event.Id });

                    // remove from queue
                    _eventQueue.Dequeue();
                }
            }
            finally
            {
                // set flag to false
                _isNotifyingSubscribers = false;
            }
        }
    }
}