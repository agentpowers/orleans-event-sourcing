using System;
using System.Threading.Tasks;
using EventSourcingGrains.Grains;
using Microsoft.Extensions.Logging;

namespace Account.Grains
{
    public class AccountGrain : EventSourceGrain<Account, IAccountEvent>, IAccountGrain
    {
        public const string AggregateName = "account";
        private readonly ILogger<AccountGrain> _logger;
        public AccountGrain(ILogger<AccountGrain> logger): base(AggregateName, new AccountAggregate())
        {
            _logger = logger;
        }

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

        public async Task<AccountResponse<decimal>> Transfer(int toAccountId, decimal amount)
        {
            // validate
            if (State.Amount - amount < 0)
            {
                return new AccountResponse<decimal>("insufficient funds", ErrorCode.InsufficientFunds);
            }
            // create transactionId
            var transactionId = Guid.NewGuid();
            // get to Account grain
            var toGrain = GrainFactory.GetGrain<IAccountGrain>(toAccountId);
            // get response
            var response = await toGrain.TransferDebit(State.AccountId, transactionId, amount);
            // save event if response was successful
            if (response.ErrorCode == ErrorCode.None)
            {
                // save event with response value(eventId) as Root and ParentId
                await ApplyEvent(
                    new TransferCredited{ AccountId = State.AccountId, ToAccountId = toAccountId, Amount = amount, TransactionId = transactionId },
                    response.Value,
                    response.Value
                );
                // return new balance
                return new AccountResponse<decimal>(State.Amount);
            }
            else 
            {
                // log info
                _logger.LogInformation($"Unable to complete transfer, Reason={response.ErrorMessage}, From={State.AccountId}, To={toAccountId}");   
                // return error 
                return new AccountResponse<decimal>(response.ErrorMessage, response.ErrorCode);
            }
        }

        public async Task<AccountResponse<long>> TransferDebit(int fromAccountId, Guid transactionId, decimal amount)
        {
            var eventId = await ApplyEvent(new TransferDebited{ AccountId = State.AccountId, FromAccountId = fromAccountId, Amount = amount, TransactionId = transactionId });
            return new AccountResponse<long>(eventId);
        }

        public async Task ReverseTransferDebit(int fromAccountId, Guid transactionId, decimal amount, long rootEventId, long parentEventId)
        {
            // TODO: make this idempotent
            await ApplyEvent(
                new TransferDebitReversed{ AccountId = State.AccountId, FromAccountId = fromAccountId, Amount = amount, TransactionId = transactionId },
                rootEventId,
                parentEventId
            );
        }
    }
}