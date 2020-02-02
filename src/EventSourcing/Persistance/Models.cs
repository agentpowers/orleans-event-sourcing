using System;

namespace EventSourcing.Persistance
{
    internal class Aggregate
    {
        public long AggregateId {get; set; }
        public string Type { get; set; }
        public DateTime Created {get; set;}
    }

    internal class Event
    {
        public long Sequence { get; set; }
        public long AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }

    public class Snapshot
    {
        public long Sequence { get; set; }
        public long AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}
