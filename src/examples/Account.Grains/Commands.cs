using System.Threading.Tasks;

namespace Account.Grains
{
    public interface IAccountCommand
    {
        Task<decimal> Deposit(decimal amount);
        Task<decimal> Withdraw(decimal amount);
        Task<decimal> GetBalance();
    }
}
