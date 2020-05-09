using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace Account.Grains
{
    [Flags]
    public enum ErrorCode
    {
        None = 0,
        Unknown = 1,
        InsufficientFunds = 2
    }
    public struct AccountResponse<T>
    {
        public T Value { get; set; }
        public string ErrorMessage { get; set; }
        public ErrorCode ErrorCode { get; set; }
        public AccountResponse(T value)
        {
            Value = value;
            ErrorMessage = null;
            ErrorCode = ErrorCode.None;
        }

        public AccountResponse(string errorMessage, ErrorCode errorCode = ErrorCode.Unknown)
        {
            Value = default(T);
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
    public interface IAccountGrain: IGrainWithIntegerKey
    {
        // return balance
        Task<AccountResponse<decimal>> GetBalance();
        // withdraw and return new balance
        Task<AccountResponse<decimal>> Withdraw(decimal amount);
        // deposit and return new balance
        Task<AccountResponse<decimal>> Deposit(decimal amount);
        // transfer to account and return new balance
        Task<AccountResponse<decimal>> Transfer(int toAccountId, decimal amount);
        // internal method - transfer debit and return eventId
        Task<AccountResponse<long>> TransferDebit(int fromAccountId, Guid transactionId, decimal amount);
        // Reverse Transfer Debited
        Task ReverseTransferDebit(int fromAccountId, Guid transactionId, decimal amount, long rootEventId, long parentEventId);
    }
}