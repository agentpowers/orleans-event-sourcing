using EventSourcing;

namespace Grains.Account
{
    public class Account : IState
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public decimal PendingCredit { get; set; }
        public decimal PendingDebit { get; set; }

        public void Init(string id)
        {
            AccountId = int.Parse(id);
        }
    }
}