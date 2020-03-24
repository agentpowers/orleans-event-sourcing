namespace EventSourcing.Stream
{
    public class AggregateStream : IAggregate<AggregateStreamState, IStreamEvent>
    {
        public AggregateStreamState State { get; set; }
        public void Apply(IStreamEvent @event)
        {
            switch (@event)
            {
                case UpdatedLastNotifiedEventId updatedLastSentEventId:
                    State.LastNotifiedEventId = updatedLastSentEventId.LastNotifiedEventId;
                    break;
                default:
                    break;
            }
        }
    }
}