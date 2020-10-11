using EventSourcing;

namespace EventSourcingGrains.Stream
{
    public interface IStreamEvent : IEvent
    {
    }

    [Event(nameof(UpdatedLastNotifiedEventId))]
    public class UpdatedLastNotifiedEventId : IStreamEvent
    {
        public long LastNotifiedEventId { get; set; }
    }
}