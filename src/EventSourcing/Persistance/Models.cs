using System;
using Orleans;

namespace EventSourcing.Persistance
{
    [GenerateSerializer]
    public sealed class Aggregate
    {
        [Id(0)]
        public long AggregateId { get; set; }
        [Id(1)]
        public string Type { get; set; }
        [Id(2)]
        public DateTime Created { get; set; }
    }

    [GenerateSerializer]
    public class AggregateEventBase
    {
        [Id(0)]
        public long Id { get; set; }
        [Id(1)]
        public long AggregateId { get; set; }
        [Id(2)]
        public long AggregateVersion { get; set; }
        [Id(3)]
        public int EventVersion { get; set; } = 0;
        [Id(4)]
        public long? RootEventId { get; set; }
        [Id(5)]
        public long? ParentEventId { get; set; }
        [Id(6)]
        public string Type { get; set; }
        [Id(7)]
        public string Data { get; set; }
        [Id(8)]
        public DateTime Created { get; set; }
    }

    [GenerateSerializer]
    public class AggregateEvent : AggregateEventBase
    {
        [Id(0)]
        public string AggregateType { get; set; }
    }

    [GenerateSerializer]
    public sealed class Snapshot
    {
        [Id(0)]
        public long Id { get; set; }
        [Id(1)]
        public long AggregateId { get; set; }
        [Id(2)]
        public long AggregateVersion { get; set; }
        [Id(3)]
        public string Data { get; set; }
        [Id(4)]
        public DateTime Created { get; set; }
    }
}
