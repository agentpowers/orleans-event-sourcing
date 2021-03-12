using System;
using System.Threading.Tasks;
using EventSourcingGrains.Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using EventSourcingGrains.Stream;
using EventSourcing.Persistance;
using System.Collections.Generic;
using Orleans.Concurrency;
using EventSourcingGrains.Keeplive;
using EventSourcing;
using Saga.Grains.EventSourcing;

namespace Saga.Grains.Manager
{
    public interface ISagaManagerReceiver : IAggregateStreamReceiver { }
    public interface ISagaManagerGrain : IKeepAliveGrain, IGrainWithStringKey, ISagaManagerReceiver
    {
    }

    [Reentrant]
    public class SagaManagerGrain : EventSourceGrain<SagaManager, ISagaManagerEvent>, ISagaManagerGrain
    {
        public const string AggregateName = "sagaManager";
        private readonly ILogger<SagaManagerGrain> _logger;
        private long _lastReceivedEventId = 0;
        private readonly HashSet<string> _runningSagas = new HashSet<string>();
        
        public SagaManagerGrain(ILogger<SagaManagerGrain> logger) : base(AggregateName, new SagaManagerAggregate())
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            //load all account events after last processed event
            await RecoverEventQueue(State.LastProcessedEventId);

            //trigger all running events
            foreach (var runningSagaId in _runningSagas)
            {
                var sagaGrain = GrainFactory.GetGrain<IBaseSagaGrain>(runningSagaId);
                await sagaGrain.Ping();
            }
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public async Task Receive(AggregateEvent @event)
        {
            if (@event.Id > _lastReceivedEventId)
            {
                // check to see if any events were missed
                if (@event.Id != _lastReceivedEventId + 1)
                {
                    await RecoverEventQueue(_lastReceivedEventId);
                    _logger.LogWarning($"Missed event, recovered={_lastReceivedEventId}, received={@event.Id}");
                    return;

                }
                // process event
                await ProcessEvent(@event);
            }
        }

        private async Task ProcessEvent(AggregateEvent @event)
        {
            var sagaEvent = EventSerializer.DeserializeEvent(@event);
            switch (sagaEvent)
            {
                case Executing executing:
                    _runningSagas.Add(executing.Id);
                    break;
                case Compensating compensating:
                    _runningSagas.Add(compensating.Id);
                    break;
                case Executed executed:
                    _runningSagas.Remove(executed.Id);
                    break;
                case StepExecuted stepExecuted when stepExecuted.ShouldSuspend:
                    _runningSagas.Remove(stepExecuted.Id);
                    break;
                case StepCompensated stepCompensated when stepCompensated.ShouldSuspend:
                    _runningSagas.Remove(stepCompensated.Id);
                    break;
                case Compensated compensated:
                    _runningSagas.Remove(compensated.Id);
                    break;
                case Suspended suspended:
                    _runningSagas.Remove(suspended.Id);
                    break;
                case Cancelled cancelled:
                    _runningSagas.Remove(cancelled.Id);
                    break;
                case Faulted faulted:
                    _runningSagas.Remove(faulted.Id);
                    break;
                default:
                    break;
            }

            _lastReceivedEventId = @event.Id;

            if (_lastReceivedEventId % 100 == 0)
            {
                await ApplyEvent(
                    new CheckPointCreated { LastProcessedEventId = _lastReceivedEventId },
                    @event.RootEventId,
                    @event.ParentEventId
                );
            }
        }

        private async Task RecoverEventQueue(long fromEventId)
        {
            var aggregateEvents = await EventSource.GetAggregateEvents(SagaGrain<object>.AggregateName, fromEventId);
            foreach (var aggregateEvent in aggregateEvents)
            {
                // progress event
                await ProcessEvent(aggregateEvent);
            }
        }
    }
}