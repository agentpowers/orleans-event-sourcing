using System;
using EventSourcing;

namespace Grains.Account.Reconciler
{
    public interface IAccountReconcilerEvent: IEvent
    {
        Guid TransactionId { get; set; }
        long EventId { get; set; }
    }

    #region Events
    public class TransactionMatched: IAccountReconcilerEvent
    {
        public string Type { get; set;} = nameof(TransactionMatched);
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }

    public class TransactionReversed: IAccountReconcilerEvent
    {
        public string Type { get; set;} = nameof(TransactionReversed);
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }

    public class ManualInterventionRequired: IAccountReconcilerEvent
    {
        public string Type { get; set;} = nameof(ManualInterventionRequired);
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }

    #endregion
}