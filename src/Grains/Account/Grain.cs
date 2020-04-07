using GrainInterfaces;
using System;
using System.Threading.Tasks;
using EventSourcing.Grains;

namespace Grains.Account
{
    public class AccountGrain : EventSourceGrain<Account, IAccountEvent>, IAccountGrain
    {
        public const string AggregateName = "account";
        public AccountGrain(): base(AggregateName, new AccountAggregate())
        {}

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public async Task<AccountResponse<decimal>> Deposit(decimal amount)
        {
            await ApplyEvent(new Deposited{ Amount = amount, AccountId = State.AccountId });
            return new AccountResponse<decimal>(State.Amount);
        }

        public Task<AccountResponse<decimal>> GetBalance()
        {
            return Task.FromResult(new AccountResponse<decimal>(State.Amount));
        }

        public async Task<AccountResponse<decimal>> Withdraw(decimal amount)
        {
            // validate
            if (State.Amount - amount < 0)
            {
                return new AccountResponse<decimal>("insufficient funds", ErrorCode.InsufficientFunds);
            }
            // save event
            await ApplyEvent(new Withdrawn{ Amount = amount, AccountId = State.AccountId});
            return new AccountResponse<decimal>(State.Amount);
        }

        public async Task<AccountResponse<decimal>> TransferTo(int toAccountId, decimal amount)
        {
            // validate
            if (State.Amount - amount < 0)
            {
                return new AccountResponse<decimal>("insufficient funds", ErrorCode.InsufficientFunds);
            }
            // create transactionId
            var transactionId = Guid.NewGuid();
            // save event
            await ApplyEvent(new TransferCredited{ AccountId = State.AccountId, ToAccountId = toAccountId, Amount = amount, TransactionId = transactionId });
            // get to grain
            var toGrain = GrainFactory.GetGrain<IAccountGrain>(toAccountId);
            // get response
            var response = await toGrain.TransferFrom(State.AccountId, transactionId, amount);
            // compensate previous event if not success
            if (!response.Value)
            {
                await ApplyEvent(new TransferCreditReversed{ AccountId = State.AccountId, ToAccountId = toAccountId, Amount = amount, TransactionId = transactionId });
            }
            return new AccountResponse<decimal>(State.Amount);
        }

        public async Task<AccountResponse<bool>> TransferFrom(int fromAccountId, Guid transactionId, decimal amount)
        {
            await ApplyEvent(new TransferDebited{ AccountId = State.AccountId, FromAccountId = fromAccountId, Amount = amount, TransactionId = transactionId });
            return new AccountResponse<bool>(true);
        }
    }
}