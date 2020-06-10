using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Microsoft.Extensions.Logging;
using EventSourcingGrains.Stream;
using EventSourcing.Persistance;
using System.Linq;

namespace EventSourcingGrains.Grains
{
    [Reentrant]
    public class AggregateStreamGrain : Grain, IAggregateStreamGrain
    {
        private IAggregateStreamSettings _aggregateStreamSettings;
        private readonly ILogger<AggregateStreamGrain> _logger;
        private readonly IRepository _repository;
        private long _lastNotifiedEventId;
        private long _lastDispatchedEventId;
        private int _skippedPollingCount;
        private string _aggregateName;
        private static TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
        public const string AggregateName = "aggregate_stream";
        private const int SkippedPollingCountThreshold = 60;

        public AggregateStreamGrain(ILogger<AggregateStreamGrain> logger, IRepository repository, IAggregateStreamSettings aggregateStreamSettings)
        {
            _logger = logger;
            _repository = repository;
            _aggregateStreamSettings = aggregateStreamSettings;
        }
        
        public override async Task OnActivateAsync()
        {
            // set current grain key as aggregateName
            _aggregateName = GrainReference.GetPrimaryKeyString();

            // call base OnActivateAsync
            await base.OnActivateAsync();

            var persistedDispatchers = _aggregateStreamSettings.EventDispatcherSettingsMap.Where(g => g.Value.PersistDispatcherState == true).ToList();

            if (persistedDispatchers.Count > 0)
            {
                // get lowest lastQueuedEventId
                foreach (var eventDispatcherSettings in persistedDispatchers)
                {
                    var dispatcherGrain = GrainFactory.GetGrain<IAggregateStreamDispatcherGrain>($"{_aggregateName}:{eventDispatcherSettings.Key}");
                    var dispatcherGrainLastNotifiedEventId = await dispatcherGrain.GetLastQueuedEventId();
                    if (_lastDispatchedEventId == 0 || _lastDispatchedEventId > dispatcherGrainLastNotifiedEventId)
                    {
                        _lastDispatchedEventId = dispatcherGrainLastNotifiedEventId;
                    }
                }
            }
            else
            {
                // get last event from db
                var lastEvent = await _repository.GetLastAggregateEvent(_aggregateName);
                _lastDispatchedEventId = lastEvent?.AggregateVersion ?? 0;
            }

            // register pollForEvents method
            this.RegisterTimer(PollForEvents, null, TimeSpan.FromSeconds(1), _pollingInterval);
        }

        /// <summary>
        /// Ping method(to keep grain alive)
        /// </summary>
        /// <returns></returns>
        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public Task Notify(long eventId)
        {
            _lastNotifiedEventId = eventId;
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method will get events that are greater than last notified eventId from database and 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task PollForEvents(object args)
        {
            // if there is no notification of new event and we haven't reached PollingSkipThreshold then break
            if(_lastNotifiedEventId <= _lastDispatchedEventId)
            {
                _skippedPollingCount++;
                if (_skippedPollingCount != SkippedPollingCountThreshold)
                {
                    return;
                }
            }
            // get new events after last Queued Event Id
            var newEvents = await _repository.GetAggregateEvents(_aggregateName, _lastDispatchedEventId);

            // reset skip count
            _skippedPollingCount = 0;

            // short circuit if no events to process
            if (newEvents.Length == 0)
            {
                return;
            }
            
            // send events to dispatcher
            foreach (var eventDispatcherSettings in _aggregateStreamSettings.EventDispatcherSettingsMap)
            {
                var dispatcherGrain = GrainFactory.GetGrain<IAggregateStreamDispatcherGrain>($"{_aggregateName}:{eventDispatcherSettings.Key}");
                await dispatcherGrain.AddToQueue(new Immutable<AggregateEvent[]>(newEvents));
            }

            // record last dispatched items id
            _lastDispatchedEventId = newEvents[newEvents.Length - 1].Id;
        }
    }
}