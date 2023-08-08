using EventSourcing;

namespace Saga.Grains.Manager
{
    public class SagaManager : IState
    {
        public long LastProcessedEventId { get; set; }

        public void Init(string key)
        {
        }
    }
}
