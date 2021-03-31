using System.Threading.Tasks;

namespace EventSourcing.Persistance
{
    public interface IRepository
    {
        Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId);
        Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId, int size);
        Task<AggregateEvent> GetLastAggregateEvent(string aggregateName);
        Task<AggregateEvent[]> GetAggregateEventsByAggregateTypeName(string aggregateName, string aggregateTypName, long aggregateVersion);
        Task<Aggregate> GetAggregateByTypeName(string type);
        Task<(Snapshot, AggregateEventBase[])> GetSnapshotAndEvents(string aggregateName, long aggregateId);
        Task<long> SaveAggregate(Aggregate aggregate);
        Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot);
        Task<long> SaveEvent(string aggregateName, AggregateEventBase @event);
        Task<long> SaveEvents(string aggregateName, params AggregateEventBase[] events);
        Task CreateEventsAndSnapshotsTables(string aggregateName);
    }
}