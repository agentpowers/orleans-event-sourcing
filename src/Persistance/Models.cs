using System;

namespace Persistance
{
    //type Aggregate = {AggregateId:Guid;Version:int;Type:string;Created:DateTime}
    public class Aggregate
    {
        public Guid AggregateId {get; set; }
        public long Version {get; set; }
        public string Type { get; set; }
        public DateTime Created {get; set;}
    }

    //type Event = {Sequence:int64;AggregateId:Guid;Version:int;Type:string;Data:string;Created:DateTime}
    public class Event
    {
        public long Sequence { get; set; }
        public Guid AggregateId { get; set; }
        public long Version { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }

    //type Snapshot = {Sequence:int64;AggregateId:Guid;Version:int;LastEventSequence:int64;Data:string;Created:DateTime}
    public class Snapshot
    {
        public long Sequence { get; set; }
        public Guid AggregateId { get; set; }
        public long Version { get; set; }
        public long LastEventSequence { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}
