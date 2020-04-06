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
        public decimal PendingAmount { get; set; }
    }

    public class Withdrawn: IAccountEvent
    {
        public string Type { get; set; } = nameof(Withdrawn);
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    public class TransferCreditPending: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferCreditPending);
        public int AccountId { get; set; }
        public int ToAccount { get; set; }
        public decimal Amount { get; set; }
        public long TransactionId { get; set; }
    }

    public class TransferDebitPending: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferDebitPending);
        public int AccountId { get; set; }
        public int FromAccount { get; set; }
        public decimal Amount { get; set; }
        public long TransactionId { get; set; }
    }

    public class TransferCreditConfirmed: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferCreditConfirmed);
        public int AccountId { get; set; }
        public int ToAccount { get; set; }
        public decimal Amount { get; set; }
        public long TransactionId { get; set; }
    }

    public class TransferDebitConfirmed: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferDebitConfirmed);
        public int AccountId { get; set; }
        public int FromAccount { get; set; }
        public decimal Amount { get; set; }
        public long TransactionId { get; set; }
    }

    #endregion
}