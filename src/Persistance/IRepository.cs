using System;
using System.Threading.Tasks;

namespace Persistance
{
    public interface IRepository
    {
        Task<Event[]> GetEvents(long aggregateId, long lastEventSequence);
        Task<(Snapshot, Event[])> GetSnapshotAndEvents(long aggregateId);
        Task<long> GetSnapshotLastEventSequence(long aggregateId);
        Task<Aggregate> GetAggregate(long id, string type);
        Task<Aggregate> GetAggregateByTypeName(string type);
        Task<Aggregate[]> GetAggregatesByTypeName(string type);
        Task<Event> GetLastEvent(long aggregateId);
        Task <long> SaveAggregate(Aggregate aggregate);
        Task<long> SaveSnapshot(Snapshot snapshot);
        Task<long> SaveEvent(Event @event);
        Task InitTables();
    }
}