using System;
using System.Threading.Tasks;

namespace EventSourcing.Persistance
{
    internal interface IRepository
    {
        Task<Event[]> GetEvents(string aggregateType, long lastEventSequence);
        Task<(Snapshot, Event[])> GetSnapshotAndEvents(string aggregateType );
        Task<long> GetSnapshotLastEventSequence(string aggregateType);
        Task<Aggregate> GetAggregate(long id, string type);
        Task<Aggregate> GetAggregateByTypeName(string type);
        Task<Aggregate[]> GetAggregatesByTypeName(string type);
        Task<Event> GetLastEvent(string aggregateType);
        Task <long> SaveAggregate(Aggregate aggregate);
        Task<long> SaveSnapshot(string aggregateType, Snapshot snapshot);
        Task<long> SaveEvent(string aggregateType, Event @event);
    }
}