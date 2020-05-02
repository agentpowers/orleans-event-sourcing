using System;
using EventSourcing;
using EventSourcingGrains;

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

    public class TransferCredited: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferCredited);
        public int AccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class TransferDebited: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferDebited);
        public int AccountId { get; set; }
        public int FromAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }
    
    public class TransferCreditReversed: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferCreditReversed);
        public int AccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class TransferDebitReversed: IAccountEvent
    {
        public string Type { get; set; } = nameof(TransferDebitReversed);
        public int AccountId { get; set; }
        public int FromAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid TransactionId { get; set; }
    }

    #endregion
}