
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
    public class AccountGrain : Grain, IAccountGrain
    {
        private AccountAggregate _accountAggregate = new AccountAggregate();
        private long _accountId;
        private string _aggregateType;
        private long _aggregateId;
        private readonly IRepository _repository;
        private Account _account = new Account();
        private int _eventCount = 0;
        private long _lastEventSequence = 0;

        public AccountGrain(IRepository repository)
        {
            _repository = repository;
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T DeSerialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
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
                _account = DeSerialize<Account>(snapshot.Data);
            }
            // apply events
            foreach (var dbEvent in events)
            {
                switch (dbEvent.Type)
                {
                    case nameof(Deposited):
                        _account = _accountAggregate.Apply(DeSerialize<Deposited>(dbEvent.Data), _account);
                        break;
                    case nameof(Withdrawn):
                        _account = _accountAggregate.Apply(DeSerialize<Withdrawn>(dbEvent.Data), _account);
                        break;
                    default:
                        break;
                }
                
            }
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public async Task<decimal> Deposit(decimal amount)
        {
            await ApplyCommand(new Deposit{ Amount = amount });
            return _account.Amount;
        }

        public Task<decimal> GetBalance()
        {
            return Task.FromResult(_account.Amount);
        }

        public Task<Result<decimal>> Transfer(int accountId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<decimal>> Withdraw(decimal amount)
        {
            await ApplyCommand(new Withdraw{ Amount = amount });
            return new Result<decimal>{ Value = _account.Amount };
        }

        private async Task ApplyCommand(IAccountCommands command)
        {
            // get updatedState and event
            var (updatedState, @event) = _accountAggregate.Exec(command, _account);
            // serialize event for db
            var serialized = Serialize(@event);
            // save event to db
            _lastEventSequence = await _repository.SaveEvent(new Event { AggregateId = _aggregateId, Type = @event.Type, Data = serialized });
            // increment event count
            _eventCount++;
            // save snapshot when needed(every 10 events)
            if (_eventCount % 10 == 0)
            {
                await _repository.SaveSnapshot(new Snapshot{ AggregateId = _aggregateId, LastEventSequence = _lastEventSequence, Data = Serialize(_account) });
            }
            // update state
            _account = updatedState;
        }
    }
}