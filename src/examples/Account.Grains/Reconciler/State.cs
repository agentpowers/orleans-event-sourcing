using EventSourcing;

namespace Account.Grains.Reconciler
{
    public class AccountReconciler : IState
    {
        public long LastProcessedEventId { get; set; }

        public void Init(string key)
        {
        }
    }
}
