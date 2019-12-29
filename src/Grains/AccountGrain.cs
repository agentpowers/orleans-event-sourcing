
using GrainInterfaces;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains
{
    public class AccountGrain : Grain, IAccountGrain
    {
        private int _balance = 0;

        public Task<int> Deposit(int amount)
        {
            _balance += amount;
            return Task.FromResult(_balance);
        }

        public Task<int> GetBalance()
        {
            return Task.FromResult(_balance);
        }

        public Task<Result<int>> Transfer(int accountId, int amount)
        {
            throw new NotImplementedException();
        }

        public Task<Result<int>> Withdraw(int amount)
        {
            if ((_balance - amount) < 0 )
            {
                return Task.FromResult(new Result<int>{ Error = "not enought balance"});
            }
            else 
            {
                _balance -= amount;
                return Task.FromResult(new Result<int>{ Success = _balance });
            }
        }
    }
}