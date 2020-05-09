using EventSourcingGrains.Grains;
using EventSourcing.Persistance;
using EventSourcingGrains.Stream;
using Orleans;
using System;
using System.Threading.Tasks;
using Account.Grains.Repositories;

namespace Account.Grains.ReadModelWriter
{
    public interface IAccountModelWriterAggregateStreamReceiver: IAggregateStreamReceiver{}

    public class AccountModelWriterGrain : ModelWriter<AccountModel, AggregateEvent>, IAccountModelWriterAggregateStreamReceiver, IGrainWithStringKey
    {
        // grain key example => {writer:account:123}
        public static readonly int keyStringAccountIdStartIndex = GrainPrefix.Length + AccountGrain.AggregateName.Length + 1;
        private readonly EventSourcing.Persistance.IRepository _eventSourcingRepository;
        private readonly IAccountRepository _accountRepository;
        private long _accountId;
        public AccountModelWriterGrain(
            EventSourcing.Persistance.IRepository eventSourcingRepository,
            IAccountRepository accountRepository)
            : base(new AccountModelAggregate())
        {
            _eventSourcingRepository = eventSourcingRepository;
            _accountRepository = accountRepository;
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
                await ApplyEvent(@event);
            }
        }

        public override async Task<(AccountModel, AggregateEvent[])> GetCurrentStateAndPendingEvents()
        {
            // get current state from db
            var currentState = await _accountRepository.GetAccount(_accountId);
            // return if null
            if(currentState == null)
            {
                // save default state
                currentState = new AccountModel{ Id = _accountId, Balance = 0, Version = 0, Modified = DateTime.UtcNow };
                await _accountRepository.CreateAccount(currentState);
            }
            
            // get events
            var events = await _eventSourcingRepository.GetAggregateEventsByAggregateTypeName(AccountGrain.AggregateName, $"{AccountGrain.AggregateName}:{_accountId}", currentState.Version);

            // return state and events
            return (currentState, events);
        }
    }
}