using EventSourcing;
using EventSourcing.Persistance;

namespace Grains.Account.ReadModelWriter
{
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
                case TransferCredited transferCredited:
                    State.Balance -= transferCredited.Amount;
                    break;
                case TransferDebited transferDebited:
                    State.Balance += transferDebited.Amount;
                    break;
                default:
                    break;
            }
        }
    }
}