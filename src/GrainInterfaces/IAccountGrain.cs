using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public class Result<T>
    {
        public T Value { get; set; }
        public string Error { get; set; }
    }
    public interface IAccountGrain : IGrainWithIntegerKey
    {
        Task<decimal> GetBalance();
        Task<Result<decimal>> Withdraw(decimal amount);
        Task<decimal> Deposit(decimal amount);
        Task<Result<decimal>> Transfer(int accountId, decimal amount);
    }
}