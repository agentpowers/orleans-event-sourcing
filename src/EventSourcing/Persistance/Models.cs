using System;
using Orleans.Concurrency;

namespace EventSourcing.Persistance
{
    public sealed class Aggregate
    {
        public long AggregateId {get; set; }
        public string Type { get; set; }
        public DateTime Created {get; set;}
    }

    public class Event
    {
        public long Id { get; set; }
        public long AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public int EventVersion { get; set; } = 0;
        public long? RootEventId { get; set; }
        public long? ParentEventId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }

    [Immutable]
    public class AggregateEvent: Event
    {
        public string AggregateType { get; set; }
    }

    public sealed class Snapshot
    {
        public long Id { get; set; }
        public long AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}
