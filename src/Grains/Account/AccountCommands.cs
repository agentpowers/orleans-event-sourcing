using System.Threading.Tasks;

namespace Grains.Account
{
    public interface IAccountCommand
    {
        Task<decimal> Deposit(decimal amount);
        Task<decimal> Withdraw(decimal amount);
        Task<decimal> GetBalance();
    }
}