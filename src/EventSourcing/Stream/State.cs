namespace EventSourcing.Stream
{
    public class AggregateStreamState: IState
    { 
        public long LastNotifiedEventId { get; set; }
        public string Id { get; set; }

        public void Init(string id)
        {
            Id = id;
        }
    }
}