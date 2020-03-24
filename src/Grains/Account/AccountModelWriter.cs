using EventSourcing;
using EventSourcing.Grains;
using EventSourcing.Persistance;
using EventSourcing.Stream;
using Grains.Repositories;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains.Account
{
    public class AccountModel
    {
        public long Version { get; set; }
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime Modified { get; set; }
    }
    
    public class AccountModelAggregate : IAggregate<AccountModel, AggregateEvent>
    {
        public AccountModel State { get; set; }
        public void Apply(AggregateEvent aggregateEvent)
        {
            State.Version = aggregateEvent.AggregateVersion;
            State.Modified = aggregateEvent.Created;
            var accountEvent = JsonSerializer.DeserializeEvent<IAccountEvent>(aggregateEvent.Data);
            switch (accountEvent)
            {
                case Deposited deposited: 
                    State.Balance += deposited.Amount;
                    break;
                case Withdrawn withdrawn:
                    State.Balance -= withdrawn.Amount;
                    break;
                case BalanceRetrieved balanceRetrieved:
                default:
                    break;
            }
        }
    }

    public interface IAccountModelAggregateStreamReceiver: IAggregateStreamReceiver{}

    public class AccountModelWriter : ModelWriter<AccountModel, AggregateEvent>, IAccountModelAggregateStreamReceiver, IGrainWithStringKey
    {
        private readonly EventSourcing.Persistance.IRepository _eventSourcingRepository;
        private readonly IAccountRepository _accountRepository;
        private long _accountId;
        public AccountModelWriter(
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