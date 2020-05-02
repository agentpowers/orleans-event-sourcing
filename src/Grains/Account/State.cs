using EventSourcing;
using EventSourcingGrains;

namespace Grains.Account
{
    public class Account : IState
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public void Init(string id)
        {
            AccountId = int.Parse(id);
        }
    }
}