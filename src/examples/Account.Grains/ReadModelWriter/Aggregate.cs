using EventSourcing.Persistance;
using EventSourcing;

namespace Account.Grains.ReadModelWriter
{
    public class AccountModelAggregate : IAggregate<AccountModel, AggregateEvent>
    {
        public AccountModel State { get; set; }
        public void Apply(AggregateEvent aggregateEvent)
        {
            State.Version = aggregateEvent.AggregateVersion;
            //State.Modified = aggregateEvent.Created;
            State.Modified = System.DateTime.UtcNow;

            var accountEvent = EventSerializer.DeserializeEvent(aggregateEvent);
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