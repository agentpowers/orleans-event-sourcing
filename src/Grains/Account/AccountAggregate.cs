using System;
using EventSourcing;

namespace Grains.Account
{
    public class AccountAggregate : IAggregate<Account, AccountEvent>
    {
        public Account State { get; set; }
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