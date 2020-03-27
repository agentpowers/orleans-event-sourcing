using EventSourcing;

namespace Grains.Account
{
    public interface IAccountEvent: IEvent
    {
        int AccountId { get; set; }
    }
    #region Events
    public class Deposited: IAccountEvent
    {
        public string Type { get; set;} = nameof(Deposited);
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    public class Withdrawn: IAccountEvent
    {
        public string Type { get; set; } = nameof(Withdrawn);
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    public class BalanceRetrieved: IAccountEvent
    {
        public string Type { get; set; } = nameof(BalanceRetrieved);
        public int AccountId { get; set; }
    }

    #endregion
}