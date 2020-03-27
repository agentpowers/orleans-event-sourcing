
using GrainInterfaces;
using System;
using System.Threading.Tasks;
using EventSourcing.Grains;

namespace Grains.Account
{
    public class AccountGrain : EventSourceGrain<Account, IAccountEvent>, IAccountGrain, IAccountCommand
    {
        public const string AggregateName = "account";
        public AccountGrain(): base(AggregateName, new AccountAggregate())
        {}

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public async Task<decimal> Deposit(decimal amount)
        {
            await ApplyEvent(new Deposited{ Amount = amount, AccountId = State.AccountId });
            return State.Amount;
        }

        // public async Task<decimal> GetBalance()
        // {
        //     await ApplyEvent(new BalanceRetrieved());
        //     return State.Amount;
        // }

        public Task<decimal> GetBalance()
        {
            return Task.FromResult(State.Amount);
        }

        public Task<decimal> Transfer(int accountId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> Withdraw(decimal amount)
        {
            await ApplyEvent(new Withdrawn{ Amount = amount, AccountId = State.AccountId});
            return State.Amount;
        }
    }
}