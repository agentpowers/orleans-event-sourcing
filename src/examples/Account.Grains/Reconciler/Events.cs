using System;
using EventSourcing;

namespace Account.Grains.Reconciler
{
    public interface IAccountReconcilerEvent : IEvent
    {
        Guid TransactionId { get; set; }
        long EventId { get; set; }
    }

    #region Events
    [Event(nameof(TransactionMatched))]
    public class TransactionMatched : IAccountReconcilerEvent
    {
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }
    [Event(nameof(TransactionReversed))]
    public class TransactionReversed : IAccountReconcilerEvent
    {
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }
    [Event(nameof(ManualInterventionRequired))]
    public class ManualInterventionRequired : IAccountReconcilerEvent
    {
        public Guid TransactionId { get; set; }
        public long EventId { get; set; }
    }

    #endregion
}
