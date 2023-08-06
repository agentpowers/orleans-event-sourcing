using EventSourcing;

namespace Saga.Grains.Manager
{
    public interface ISagaManagerEvent : IEvent
    {
        long LastProcessedEventId { get; set; }
    }

    #region Events
    [Event(nameof(CheckPointCreated))]
    public class CheckPointCreated : ISagaManagerEvent
    {
        public long LastProcessedEventId { get; set; }
    }

    #endregion
}
