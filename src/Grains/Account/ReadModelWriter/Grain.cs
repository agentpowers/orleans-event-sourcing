using EventSourcing.Grains;
using EventSourcing.Persistance;
using EventSourcing.Stream;
using Grains.Repositories;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains.Account.ReadModelWriter
{
    public interface IAccountModelWriterAggregateStreamReceiver: IAggregateStreamReceiver{}

    public class AccountModelWriterGrain : ModelWriter<AccountModel, AggregateEvent>, IAccountModelWriterAggregateStreamReceiver, IGrainWithStringKey
    {
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
            _accountId = long.Parse(this.GetPrimaryKeyString().Substring(AccountGrain.AggregateName.Length + 1));
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