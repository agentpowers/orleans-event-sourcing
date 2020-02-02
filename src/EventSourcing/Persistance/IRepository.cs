using System;
using System.Threading.Tasks;

namespace EventSourcing.Persistance
{
    internal interface IRepository
    {
        Task<Event[]> GetEvents(string aggregateName, long aggregateId, long AggregateVersion);
        Task<(Snapshot, Event[])> GetSnapshotAndEvents(string aggregateName, long aggregateId);
        Task<long> GetSnapshotAggregateVersionSql(string aggregateName, long aggregateId);
        Task<Aggregate> GetAggregate(long id, string type);
        Task<Aggregate> GetAggregateByTypeName(string type);
        Task<Aggregate[]> GetAggregatesByTypeName(string type);
        Task<Event> GetLastEvent(string aggregateName, long aggregateId);
        Task <long> SaveAggregate(string aggregateName, Aggregate aggregate);
        Task SaveSnapshot(string aggregateName, Snapshot snapshot);
        Task SaveEvent(string aggregateName, Event @event);
    }
}