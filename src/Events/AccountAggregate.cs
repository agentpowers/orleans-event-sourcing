using System;

namespace Events
{
    public class AccountAggregate : IAggregate<Account, IAccountCommands, IAccountEvents>
    {
        public Account Apply(IAccountEvents @event, Account state)
        {
            switch (@event)
            {
                case Deposited deposited: 
                    state.Amount += deposited.Amount;
                    break;
                case Withdrawn withdrawn:
                    state.Amount -= withdrawn.Amount;
                    break;
                case BalanceRetrieved balanceRetrieved:
                default:
                    break;
            }
            return state;
        }

        public (Account, IAccountEvents) Exec(IAccountCommands command, Account state)
        {
            switch (command)
            {
                case Deposit deposit:
                    var deposited = new Deposited{ Amount = deposit.Amount };
                    return (Apply(deposited, state), deposited);
                case Withdraw withdraw:
                    var withdrawn = new Withdrawn{ Amount = withdraw.Amount };
                    return (Apply(withdrawn, state), withdrawn);
                case GetBalance getBalance:
                    var balanceRetrieved = new BalanceRetrieved();
                    return (Apply(balanceRetrieved, state), balanceRetrieved);
                default: 
                    throw new InvalidOperationException("invalid command");
            }
        }
    }
}