using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing;
using EventSourcing.Persistance;
using EventSourcingGrains.Grains;
using EventSourcingGrains.Keeplive;
using EventSourcingGrains.Stream;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;

namespace Account.Grains.Reconciler
{
    public interface IAccountReconcilerReceiver : IAggregateStreamReceiver { }
    public interface IAccountReconcilerGrain : IKeepAliveGrain, IGrainWithStringKey, IAccountReconcilerReceiver
    {
    }

    [Reentrant]
    public class AccountReconcilerGrain : EventSourceGrain<AccountReconciler, IAccountReconcilerEvent>, IAccountReconcilerGrain
    {
        public const string AggregateName = "accountReconciler";
        private readonly ILogger<AccountReconcilerGrain> _logger;
        private readonly TimeSpan _reverseTransactionWaitPeriod = TimeSpan.FromMinutes(2);
        // flag indicating if tranfer debited event queue is being processed
        private bool _isProcessingTransferDebitedEventQueue = false;
        // flag indicating if event queue is being processed
        private bool _isProcessingEventQueue = false;
        private long _lastQueuedEventId = 0;
        // local state to keep unmatched transactions
        private readonly HashSet<Guid> _unmatchedTransactions = new HashSet<Guid>();
        // AggregateEvent queue to process them in background
        private readonly Queue<AggregateEvent> _eventQueue = new Queue<AggregateEvent>();
        // queue with TransferDebitedEvent TransactionId and AggregateEvent
        private readonly Queue<(Guid TransactionId, AggregateEvent AggregateEvent)> _transferDebitedEventQueue = new Queue<(Guid, AggregateEvent)>();
        public AccountReconcilerGrain(ILogger<AccountReconcilerGrain> logger) : base(AggregateName, new AccountReconcilerAggregate())
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            //load all account events after last processed event
            await RecoverEventQueue(State.LastProcessedEventId);
            //initialize timer to clear queue
            RegisterTimer(ProcessQueue, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            //initialize timer to clear transferDebitedEventQueue
            RegisterTimer(ProcessTransferDebitedEventQueue, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        //TODO: setup keep alive
        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public async Task Receive(AggregateEvent @event)
        {
            if (@event.Id > _lastQueuedEventId)
            {
                // check to see if any events were missed
                if (@event.Id != _lastQueuedEventId + 1)
                {
                    await RecoverEventQueue(_lastQueuedEventId);
                    _logger.LogWarning($"Missed event, recovered={_lastQueuedEventId}, received={@event.Id}");
                    return;

                }
                // add to queue
                _eventQueue.Enqueue(@event);
                _lastQueuedEventId = @event.Id;
            }
        }

        private async Task ProcessQueue(object args)
        {
            // if already processing then return
            if (_isProcessingEventQueue)
            {
                return;
            }

            // set flag to true
            _isProcessingEventQueue = true;

            try
            {
                while (_eventQueue.Count > 0)
                {
                    var @event = _eventQueue.Peek();
                    var accountEvent = EventSerializer.DeserializeEvent(@event);
                    switch (accountEvent)
                    {
                        case TransferCredited transferCredited:
                            await RemoveFromUnmatched(transferCredited.TransactionId, @event);
                            break;
                        case TransferDebitReversed transferDebitReversed:
                            await RemoveFromUnmatched(transferDebitReversed.TransactionId, @event);
                            break;
                        case TransferDebited transferDebited:
                            // add to queue
                            _transferDebitedEventQueue.Enqueue((transferDebited.TransactionId, @event));
                            // add to unmatched hashset
                            _unmatchedTransactions.Add(transferDebited.TransactionId);
                            break;
                        case TransferCreditReversed transferCreditReversed:
                            //this event shouldn't happen(not implemented)
                            await ApplyEvent(
                                new ManualInterventionRequired { TransactionId = transferCreditReversed.TransactionId, EventId = @event.Id },
                                @event.RootEventId,
                                @event.Id
                            );
                            break;
                        default:
                            break;
                    }
                    // remove item from queue
                    _eventQueue.Dequeue();
                }
            }
            finally
            {
                _isProcessingEventQueue = false;
            }
        }

        private async Task RemoveFromUnmatched(Guid transactionId, AggregateEvent @event)
        {
            // remove from dictionary
            if (_unmatchedTransactions.Remove(transactionId))
            {
                await ApplyEvent(
                    new TransactionMatched { TransactionId = transactionId, EventId = @event.Id },
                    @event.RootEventId,
                    @event.ParentEventId
                );
            }
            else
            {
                //transaction was reversed already or something catastrophic
                await ApplyEvent(
                    new ManualInterventionRequired { TransactionId = transactionId, EventId = @event.Id },
                    @event.RootEventId,
                    @event.ParentEventId
                );
            }
        }

        private async Task ProcessTransferDebitedEventQueue(object args)
        {
            // if already processing then return
            if (_isProcessingTransferDebitedEventQueue)
            {
                return;
            }
            // set flag to true
            _isProcessingTransferDebitedEventQueue = true;

            try
            {

                while (_transferDebitedEventQueue.Count > 0)
                {
                    // get next item in queue
                    var (transactionId, nextEventToProcess) = _transferDebitedEventQueue.Peek();

                    // has enough time passed to start processing the item
                    if (nextEventToProcess.Created.Add(_reverseTransactionWaitPeriod) > DateTime.UtcNow)
                    {
                        break;
                    }

                    // check to see if this transactionId is in unmatched transactions
                    if (_unmatchedTransactions.Contains(transactionId))
                    {
                        // reverse transaction
                        // deserialize event
                        var transferDebitedEvent = (TransferDebited)EventSerializer.DeserializeEvent(nextEventToProcess);
                        // get grain
                        var account = GrainFactory.GetGrain<IAccountGrain>(transferDebitedEvent.AccountId);
                        // reverse and get eventId back
                        await account.ReverseTransferDebit(transferDebitedEvent.FromAccountId, transferDebitedEvent.TransactionId, transferDebitedEvent.Amount, nextEventToProcess.Id, nextEventToProcess.Id);
                    }

                    // remove item from queue
                    _transferDebitedEventQueue.Dequeue();
                }
            }
            finally
            {
                _isProcessingTransferDebitedEventQueue = false;
            }
        }

        private async Task RecoverEventQueue(long fromEventId)
        {
            var aggregateEvents = await EventSource.GetAggregateEvents(AccountGrain.AggregateName, fromEventId);
            foreach (var aggregateEvent in aggregateEvents)
            {
                // add event to queue
                _eventQueue.Enqueue(aggregateEvent);
                _lastQueuedEventId = aggregateEvent.Id;
            }
        }
    }
}
