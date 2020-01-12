using System;

namespace Persistance
{
    public class Aggregate
    {
        public long AggregateId {get; set; }
        public int Version {get; set; }
        public string Type { get; set; }
        public DateTime Created {get; set;}
    }

    public class Event
    {
        public long Sequence { get; set; }
        public long AggregateId { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }

    public class Snapshot
    {
        public long Sequence { get; set; }
        public long AggregateId { get; set; }
        public int Version { get; set; }
        public long LastEventSequence { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}
