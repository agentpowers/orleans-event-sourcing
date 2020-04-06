using EventSourcing;

namespace Grains.Account
{
    public class AccountAggregate : IAggregate<Account, IAccountEvent>
    {
        public Account State { get; set; }
        public void Apply(IAccountEvent @event)
        {
            switch (@event)
            {
                case Deposited deposited: 
                    State.Amount += deposited.Amount;
                    break;
                case Withdrawn withdrawn:
                    State.Amount -= withdrawn.Amount;
                    break;
                case TransferCreditPending transferCreditPending:
                    State.Amount -= transferCreditPending.Amount;
                    State.PendingCredit += transferCreditPending.Amount;
                    break;
                case TransferDebitPending transferDebitPending:
                    State.PendingDebit += transferDebitPending.Amount;
                    break;
                case TransferCreditConfirmed transferCreditConfirmed:
                    State.PendingCredit -= transferCreditConfirmed.AccountId;
                    break;
                case TransferDebitConfirmed transferDebitConfirmed:
                    State.Amount += transferDebitConfirmed.Amount;
                    State.PendingDebit -= transferDebitConfirmed.Amount;
                    break;
                default:
                    break;
            }
        }
    }
}