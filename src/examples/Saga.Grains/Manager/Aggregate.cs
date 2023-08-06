using EventSourcing;

namespace Saga.Grains.Manager
{
    public class SagaManagerAggregate : IAggregate<SagaManager, ISagaManagerEvent>
    {
        public SagaManager State { get; set; }
        public void Apply(ISagaManagerEvent @event)
        {
            State.LastProcessedEventId = @event switch
            {
                _ => @event.LastProcessedEventId,
            };
        }
    }
}
