using System;
using EventSourcing;

namespace Account.Grains
{
    public interface IAccountEvent: IEvent
    {
        int AccountId { get; set; }
    }

    #region Events
    [Event(nameof(Deposited))]
    public class Deposited: IAccountEvent
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public decimal PendingAmount { get; set; }
    }

    [Event(nameof(Withdrawn))]
    public class Withdrawn: IAccountEvent
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    [Event(nameof(TransferCredited))]
    public class TransferCredited: IAccountEvent
    {
        public int AccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    [Event(nameof(TransferDebited))]
    public class TransferDebited: IAccountEvent
    {
        public int AccountId { get; set; }
        public int FromAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }
    
    [Event(nameof(TransferCreditReversed))]
    public class TransferCreditReversed: IAccountEvent
    {
        public int AccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    [Event(nameof(TransferDebitReversed))]
    public class TransferDebitReversed: IAccountEvent
    {
        public int AccountId { get; set; }
        public int FromAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    #endregion
}