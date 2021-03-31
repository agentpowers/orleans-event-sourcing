using EventSourcing;
using EventSourcingGrains;

namespace Account.Grains.Reconciler
{
    public class AccountReconcilerAggregate : IAggregate<AccountReconciler, IAccountReconcilerEvent>
    {
        public AccountReconciler State { get; set; }
        public void Apply(IAccountReconcilerEvent @event)
        {
            State.LastProcessedEventId = @event switch
            {
                _ => @event.EventId,
            };
        }
    }
}