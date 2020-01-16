
using GrainInterfaces;
using Orleans;
using System;
using System.Threading.Tasks;
using Events;
using Persistance;
using Newtonsoft.Json;
using System.Linq;

namespace Grains
{
    public class AccountGrain : Grain, IAccountGrain, IAccountCommand
    {
        private AccountAggregate _accountAggregate = null;
        private long _accountId;
        private string _aggregateType;
        private long _aggregateId;
        private readonly IRepository _repository;
        private int _eventCount = 0;
        private long _lastEventSequence = 0;
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public AccountGrain(IRepository repository)
        {
            _repository = repository;
        }

        public static string SerializeEvent<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, serializerSettings);
        }

        public static T DeserializeEvent<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        public override async Task OnActivateAsync()
        {
            // get account id from grain
            _accountId = this.GetGrainIdentity().PrimaryKeyLong;
            // generate aggregateType
            _aggregateType = $"Account:{_accountId}";
            // get aggregate from db
            var aggregate = await _repository.GetAggregateByTypeName(_aggregateType);
            if (aggregate == null)
            {
                // add new aggregate if it doesn't exist
                _aggregateId = await _repository.SaveAggregate(new Aggregate{ Type = _aggregateType });
            }
            else
            {
                // use aggregate id from db
                _aggregateId = aggregate.AggregateId;
            }
            // get snapshot and events
            var (snapshot, events) = await _repository.GetSnapshotAndEvents(_aggregateId);
            // apply snapshot if any
            if(snapshot != null)
            {
                _accountAggregate = new AccountAggregate(JsonConvert.DeserializeObject<Account>(snapshot.Data));
            }
            else
            {
                _accountAggregate = new AccountAggregate(new Account());
            }
            // apply events
            foreach (var dbEvent in events)
            {
                var @event = DeserializeEvent<Events.AccountEvent>(dbEvent.Data);
                _accountAggregate.Apply(@event);
            }
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public async Task<decimal> Deposit(decimal amount)
        {
            await ApplyEvent(new Deposited{ Amount = amount });
            return _accountAggregate.State.Amount;
        }

        public async Task<decimal> GetBalance()
        {
            await ApplyEvent(new BalanceRetrieved());
            return _accountAggregate.State.Amount;
        }

        public Task<decimal> Transfer(int accountId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> Withdraw(decimal amount)
        {
            await ApplyEvent(new Withdrawn{ Amount = amount });
            return _accountAggregate.State.Amount;
        }

        private async Task ApplyEvent(AccountEvent @event)
        {
            // serialize event for db
            var serialized = SerializeEvent(@event);
            // save event to db
            _lastEventSequence = await _repository.SaveEvent(new Persistance.Event { AggregateId = _aggregateId, Type = @event.Type, Data = serialized });
            // increment event count
            _eventCount++;
            // save snapshot when needed(every 10 events)
            if (_eventCount % 10 == 0)
            {
                await _repository.SaveSnapshot(new Snapshot{ AggregateId = _aggregateId, LastEventSequence = _lastEventSequence, Data = JsonConvert.SerializeObject(_accountAggregate.State) });
            }
            // update state
            _accountAggregate.Apply(@event);
        }
    }
}