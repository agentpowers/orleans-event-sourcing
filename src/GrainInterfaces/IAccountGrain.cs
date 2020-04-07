using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace GrainInterfaces
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
        Task<AccountResponse<decimal>> GetBalance();
        Task<AccountResponse<decimal>> Withdraw(decimal amount);
        Task<AccountResponse<decimal>> Deposit(decimal amount);
        Task<AccountResponse<decimal>> TransferTo(int toAccountId, decimal amount);
        Task<AccountResponse<bool>> TransferFrom(int fromAccountId, Guid transactionId, decimal amount);
    }
}