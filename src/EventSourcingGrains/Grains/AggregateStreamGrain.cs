using Orleans;
using System;
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
        private readonly IAggregateStreamSettings _aggregateStreamSettings;
        private readonly ILogger<AggregateStreamGrain> _logger;
        private readonly IRepository _repository;
        private long _lastNotifiedEventId;
        private long _lastDispatchedEventId;
        private int _skippedPollingCount;
        private string _aggregateName;
        private bool _isPollingForEvents;
        private bool _isDispatcherUnderPressure;
        private static readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
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
                _lastDispatchedEventId = lastEvent?.Id ?? 0;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Activated stream grain, lastDispatchedEventId={_lastDispatchedEventId}, aggregateGrainName={_aggregateName}");
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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Received notification, eventId={eventId}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method will get events that are greater than last notified eventId from database and push them to dispatchers
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task PollForEvents(object args)
        {
            if (_isPollingForEvents)
            {
                return;
            }
            _isPollingForEvents = true;
            try
            {
                // if there is no notification of new event and haven't reached PollingSkipThreshold then break
                if (_lastNotifiedEventId <= _lastDispatchedEventId)
                {
                    _skippedPollingCount++;
                    if (_skippedPollingCount != SkippedPollingCountThreshold)
                    {
                        return;
                    }
                }
                // get new events after last Queued Event Id
                var newEvents = await _repository.GetAggregateEvents(_aggregateName, _lastDispatchedEventId, _aggregateStreamSettings.QueryFetchSizeLimit);

                // reset skip count
                _skippedPollingCount = 0;

                // short circuit if no events to process
                if (newEvents.Length == 0)
                {
                    return;
                }

                // logic to handle missing event id
                int i;
                for (i = newEvents.Length - 1; i >= 0; i--)
                {
                    if (newEvents[i].Id == _lastDispatchedEventId + (i + 1))
                    {
                        break;
                    }
                }

                if (i != newEvents.Length - 1)
                {
                    _logger.LogWarning($"Missed event, missingId={newEvents[i].Id + 1}, index={i}, length={newEvents.Length}");
                    newEvents = newEvents.AsSpan().Slice(0, i).ToArray();
                }

                // send events to dispatcher
                if (!_isDispatcherUnderPressure)
                {
                    foreach (var eventDispatcherSettings in _aggregateStreamSettings.EventDispatcherSettingsMap)
                    {
                        var dispatcherGrainName = $"{_aggregateName}:{eventDispatcherSettings.Key}";
                        var dispatcherGrain = GrainFactory.GetGrain<IAggregateStreamDispatcherGrain>(dispatcherGrainName);
                        // send to dispatcher and get flag indicating if dispatcher is under pressure
                        var isDispatcherUnderPressure = await dispatcherGrain.AddToQueue(new Immutable<AggregateEvent[]>(newEvents));
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug($"Send to dispatcher, aggregateName={_aggregateName}, dispatcher={dispatcherGrain}, eventIds={String.Join(',', newEvents.Select(g => g.Id))}");
                        }

                        // update under-pressure flag
                        if (_aggregateStreamSettings.ShouldHandleBackPressure && isDispatcherUnderPressure)
                        {
                            _isDispatcherUnderPressure = true;
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"Dispatcher under pressure, dispatcher={dispatcherGrainName}");
                            }
                        }
                    }

                    // record last dispatched items id
                    _lastDispatchedEventId = newEvents[newEvents.Length - 1].Id;
                }
                else
                {
                    // back pressure logic.  if all dispatchers returns false then change flag to false
                    for (int k = _aggregateStreamSettings.EventDispatcherSettingsMap.Count - 1; k >= 0; k--)
                    {
                        var dispatcherGrainName = $"{_aggregateName}:{_aggregateStreamSettings.EventDispatcherSettingsMap.Keys.ElementAt(k)}";
                        var dispatcherGrain = GrainFactory.GetGrain<IAggregateStreamDispatcherGrain>(dispatcherGrainName);
                        if (await dispatcherGrain.IsUnderPressure())
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"Dispatcher still under pressure, dispatcher={dispatcherGrainName}");
                            }
                            break;
                        }

                        if (k == 0)
                        {
                            // revert under-pressure flag
                            _isDispatcherUnderPressure = false;
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"Dispatcher recovered, dispatcher={dispatcherGrainName}");
                            }
                        }
                    }
                }
            }
            finally
            {
                _isPollingForEvents = false;
            }
        }
    }
}