using System;

namespace Events
{
    public class AccountAggregate : IAggregate<Account, AccountEvent>
    {
        public Account State { get;}
        public AccountAggregate(Account account) 
        {
            State = account;
        }
        public void Apply(AccountEvent @event)
        {
            switch (@event)
            {
                case Deposited deposited: 
                    State.Amount += deposited.Amount;
                    break;
                case Withdrawn withdrawn:
                    State.Amount -= withdrawn.Amount;
                    break;
                case BalanceRetrieved balanceRetrieved:
                default:
                    break;
            }
        }
    }
}