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
                case TransferCreditPending transferCreditPending:
                    State.Balance -= transferCreditPending.Amount;
                    State.PendingCredit += transferCreditPending.Amount;
                    break;
                case TransferDebitPending transferDebitPending:
                    State.PendingDebit += transferDebitPending.Amount;
                    break;
                case TransferCreditConfirmed transferCreditConfirmed:
                    State.PendingCredit -= transferCreditConfirmed.AccountId;
                    break;
                case TransferDebitConfirmed transferDebitConfirmed:
                    State.Balance += transferDebitConfirmed.Amount;
                    State.PendingDebit -= transferDebitConfirmed.Amount;
                    break;
                default:
                    break;
            }
        }
    }
}