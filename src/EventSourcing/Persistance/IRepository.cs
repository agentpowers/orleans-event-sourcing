using System;
using System.Threading.Tasks;

namespace EventSourcing.Persistance
{
    public interface IRepository
    {
        Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long eventId);
        Task<AggregateEvent> GetLastAggregateEvent(string aggregateName);
        Task<(Snapshot, Event[])> GetSnapshotAndEvents(string aggregateName, long aggregateId);
        Task<long> GetSnapshotAggregateVersionSql(string aggregateName, long aggregateId);
        Task<Aggregate> GetAggregate(long id, string type);
        Task<Aggregate> GetAggregateByTypeName(string type);
        Task<Aggregate[]> GetAggregatesByTypeName(string type);
        Task<Event> GetLastEvent(string aggregateName, long aggregateId);
        Task <long> SaveAggregate(string aggregateName, Aggregate aggregate);
        Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot);
        Task<long> SaveEvent(string aggregateName, Event @event);
    }
}