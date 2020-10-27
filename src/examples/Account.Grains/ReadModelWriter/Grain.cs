using EventSourcingGrains.Grains;
using EventSourcing.Persistance;
using EventSourcingGrains.Stream;
using Orleans;
using System;
using System.Threading.Tasks;
using Account.Grains.Repositories;
using Microsoft.Extensions.Logging;

namespace Account.Grains.ReadModelWriter
{
    public interface IAccountModelWriterAggregateStreamReceiver : IAggregateStreamReceiver { }

    public class AccountModelWriterGrain : ModelWriter<AccountModel, AggregateEvent>, IAccountModelWriterAggregateStreamReceiver, IGrainWithStringKey
    {
        // grain key example => {writer:account:123}
        public static readonly int keyStringAccountIdStartIndex = GrainPrefix.Length + AccountGrain.AggregateName.Length + 1;
        private readonly EventSourcing.Persistance.IRepository _eventSourcingRepository;
        private readonly IAccountRepository _accountRepository;
        private long _accountId;
        private readonly ILogger<AccountModelWriterGrain> _logger;
        public AccountModelWriterGrain(
            EventSourcing.Persistance.IRepository eventSourcingRepository,
            IAccountRepository accountRepository,
            ILogger<AccountModelWriterGrain> logger)
            : base(new AccountModelAggregate())
        {
            _eventSourcingRepository = eventSourcingRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            _accountId = long.Parse(this.GetPrimaryKeyString().Substring(keyStringAccountIdStartIndex));
            await Init();
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public override Task PersistState(AccountModel account)
        {
            return _accountRepository.UpdateAccount(account);
        }

        public async Task Receive(AggregateEvent @event)
        {
            // idempotency
            if (@event.AggregateVersion > State.Version)
            {
                // check to see if any events were missed
                if (@event.AggregateVersion != State.Version + 1)
                {
                    // restore state by getting new events from repository(this will not save state.  ApplyEvent below will save)
                    await RecoverState();
                    _logger.LogWarning($"Missed event, recovered={State.Version}, received={@event.AggregateVersion}");

                }
                await ApplyEvent(@event);
            }
        }

        public override async Task<AccountModel> GetCurrentState()
        {
            // get current state from db
            var currentState = await _accountRepository.GetAccount(_accountId);
            // return if null
            if (currentState == null)
            {
                // save default state
                currentState = new AccountModel { Id = _accountId, Balance = 0, Version = 0, Modified = DateTime.UtcNow };
                await _accountRepository.CreateAccount(currentState);
            }

            return currentState;
        }

        public override async Task<AggregateEvent[]> GetPendingEvents(long currentVersion)
        {
            // get events
            var events = await _eventSourcingRepository.GetAggregateEventsByAggregateTypeName(AccountGrain.AggregateName, $"{AccountGrain.AggregateName}:{_accountId}", currentVersion);

            // return state and events
            return events;
        }
    }
}