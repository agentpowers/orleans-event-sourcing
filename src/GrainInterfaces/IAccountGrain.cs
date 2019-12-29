using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public class Result<T>
    {
        public T Success { get; set; }
        public string Error { get; set; }
    }
    public interface IAccountGrain : IGrainWithIntegerKey
    {
        Task<int> GetBalance();
        Task<Result<int>> Withdraw(int amount);
        Task<int> Deposit(int amount);
        Task<Result<int>> Transfer(int accountId, int amount);
    }
}