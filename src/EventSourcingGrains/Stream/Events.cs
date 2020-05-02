using EventSourcing;

namespace EventSourcingGrains.Stream
{
    public interface IStreamEvent : IEvent
    {
    }

    public class UpdatedLastNotifiedEventId: IStreamEvent
    {
        public string Type { get; set;} = nameof(UpdatedLastNotifiedEventId);
        public long LastNotifiedEventId { get; set; }
    }
}