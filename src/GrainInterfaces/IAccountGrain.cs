using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface IAccountGrain : IGrainWithIntegerKey
    {
        Task<decimal> GetBalance();
        Task<decimal> Withdraw(decimal amount);
        Task<decimal> Deposit(decimal amount);
        Task<decimal> Transfer(int accountId, decimal amount);
    }
}