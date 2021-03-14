using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace EventSourcing.Persistance
{
    internal class SyncWrapper<T>
    {
        public List<T> Values { get; set; } = new List<T>();
        public readonly object SyncRoot = new object();
    }
    public class InMemoryRepository : IRepository
    {
        private static readonly Dictionary<string, SyncWrapper<AggregateEvent>> _aggregateEvents = new Dictionary<string, SyncWrapper<AggregateEvent>>();
        private static readonly Dictionary<long, Aggregate> _aggregateById = new Dictionary<long, Aggregate>();
        private static readonly Dictionary<string, Aggregate> _aggregateByType = new Dictionary<string, Aggregate>();
        private static readonly Dictionary<string, SyncWrapper<Snapshot>> _snapshotes = new Dictionary<string, SyncWrapper<Snapshot>>();

        private readonly object _syncRoot = new object();
        public Task<Aggregate> GetAggregateByTypeName(string type)
        {
            if (_aggregateByType.TryGetValue(type, out var entity))
            {
                return Task.FromResult(entity);
            }
            return Task.FromResult(default(Aggregate));
        }

        public Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId)
        {
            if (_aggregateEvents.TryGetValue(aggregateName, out var entity))
            {
                return Task.FromResult(entity.Values.Where(g => g.Id > afterEventId).ToArray());
            }
            return Task.FromResult(Array.Empty<AggregateEvent>());
        }

        public Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId, int size)
        {
            if (_aggregateEvents.TryGetValue(aggregateName, out var entity))
            {
                return Task.FromResult(entity.Values.Where(g => g.Id > afterEventId).Take(size).ToArray());
            }
            return Task.FromResult(Array.Empty<AggregateEvent>());
        }

        public Task<(Snapshot, AggregateEventBase[])> GetSnapshotAndEvents(string aggregateName, long aggregateId)
        {
            var snapshot = _snapshotes[aggregateName].Values.Where(g => g.AggregateId == aggregateId).FirstOrDefault();
            var events = _aggregateEvents[aggregateName].Values.Where(g => g.AggregateId == aggregateId & g.AggregateVersion == (snapshot?.AggregateVersion).GetValueOrDefault()).Cast<AggregateEventBase>().ToArray();
            return Task.FromResult((snapshot, events));
        }

        public Task<long> SaveAggregate(Aggregate aggregate)
        {
            lock(_syncRoot)
            {
                var id = _aggregateById.Count == 0 ? 1 : _aggregateById.Keys.OrderByDescending(g => g).First() + 1;
                aggregate.AggregateId = id;
                _aggregateById.TryAdd(id, aggregate);
                _aggregateByType.TryAdd(aggregate.Type, aggregate);
                return Task.FromResult(id);
            }
        }

        public Task<long> SaveEvent(string aggregateName, AggregateEventBase @event)
        {
            if(_aggregateEvents.TryGetValue(aggregateName, out var entity))
            {
                lock(entity.SyncRoot)
                {
                    var id = entity.Values.Count == 0 ? 1 : entity.Values.Last().Id + 1;
                    var aggregateType = _aggregateById[@event.AggregateId];
                    var aggregate = new AggregateEvent
                    {
                        Id = id,
                        AggregateId = @event.AggregateId,
                        AggregateVersion = @event.AggregateVersion,
                        EventVersion = @event.EventVersion,
                        RootEventId = @event.RootEventId,
                        ParentEventId = @event.ParentEventId,
                        Type = @event.Type,
                        Data = @event.Data,
                        Created = @event.Created,
                        AggregateType = aggregateName
                    };
                    entity.Values.Add(aggregate);
                    return Task.FromResult(id);
                }
            }
            throw new InvalidOperationException($"Message=AggregateEvent not found for {aggregateName}");
        }

        public async Task<long> SaveEvents(string aggregateName, params AggregateEventBase[] events)
        {
            long lastId = 0;
            foreach (var @event in events)
            {
                lastId = await SaveEvent(aggregateName, @event);
            }
            return lastId;
        }

        public Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot)
        {
            if(_snapshotes.TryGetValue(aggregateName, out var entity))
            {
                lock(entity.SyncRoot)
                {
                    var id = entity.Values.Count == 0 ? 1 : entity.Values.Last().Id + 1;
                    snapshot.Id = id;
                    entity.Values.Add(snapshot);
                    return Task.FromResult(id);
                }
            }
            throw new InvalidOperationException($"Message=Snapshot not found for {aggregateName}");
        }

        public Task<AggregateEvent> GetLastAggregateEvent(string aggregateName)
        {
            if (_aggregateEvents.TryGetValue(aggregateName, out var entity))
            {
                return Task.FromResult(entity.Values.OrderByDescending(g => g.Id).FirstOrDefault());
            }
            return Task.FromResult(default(AggregateEvent));
        }

        public Task<AggregateEvent[]> GetAggregateEventsByAggregateTypeName(string aggregateName, string aggregateTypeName, long aggregateVersion)
        {
            if (_aggregateEvents.TryGetValue(aggregateName, out var entity))
            {
                return Task.FromResult(entity.Values.Where(g => g.AggregateType == aggregateTypeName && g.AggregateVersion > aggregateVersion).ToArray());
            }
            return Task.FromResult(Array.Empty<AggregateEvent>());
        }

        public Task CreateEventsAndSnapshotsTables(string aggregateName)
        {
            lock(_syncRoot)
            {
                _aggregateEvents.TryAdd(aggregateName, new SyncWrapper<AggregateEvent>());
                _snapshotes.TryAdd(aggregateName, new SyncWrapper<Snapshot>());
            }
            return Task.CompletedTask;
        }
    }
}