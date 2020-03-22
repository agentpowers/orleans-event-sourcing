using System;
using System.Data;
using Dapper;
using System.Threading.Tasks;
using Npgsql;
using System.Linq;

namespace EventSourcing.Persistance
{
    internal class Repository : IRepository
    {
        private string _connectionString;
        private IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Aggregate> GetAggregate(long id, string type)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<Aggregate>(Queries.GetAggregateSql, new { id, type });
            }
        }

        public async Task<Aggregate> GetAggregateByTypeName(string type)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<Aggregate>(Queries.GetAggregateByTypeSql, new { type });
            }
        }

        public async Task<Aggregate[]> GetAggregatesByTypeName(string type)
        {
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<Aggregate>(Queries.GetAggregatesByTypeSql, new { type })).ToArray();
            }
        }

        public async Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long eventId)
        {
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<AggregateEvent>(Queries.GetAggregateEventsSql(aggregateName), new { id = eventId })).ToArray();
            }
        }

        public async Task<Event> GetLastEvent(string aggregateName, long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<Event>(Queries.GetLastEventsSql(aggregateName), new { id = aggregateId });
            }
        }

        public async Task<(Snapshot, Event[])> GetSnapshotAndEvents(string aggregateName, long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                var snapshot  = await conn.QueryFirstOrDefaultAsync<Snapshot>(Queries.GetSnapshotSql(aggregateName), new { id = aggregateId });
                return (snapshot, (await conn.QueryAsync<Event>(Queries.GetEventsByAggregateIdSql(aggregateName), new { id = aggregateId, aggregateVersion = (snapshot?.AggregateVersion).GetValueOrDefault() })).ToArray());
            }
        }

        public async Task<long> GetSnapshotAggregateVersion(string aggregateName, long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<long>(Queries.GetSnapshotAggregateVersionSql(aggregateName), new { id = aggregateId });
            }
        }

        public async Task<long> SaveAggregate(string aggregateName, Aggregate aggregate)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(Queries.NewAggregate(aggregateName), aggregate);
            }
        }

        public async Task<long> SaveEvent(string aggregateName, Event @event)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(Queries.InsertEventSql(aggregateName), @event);
            }
        }

        public async Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteAsync(Queries.InsertSnapShotSql(aggregateName), snapshot);
            }
        }

        public async Task<AggregateEvent> GetLastAggregateEvent(string aggregateName)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<AggregateEvent>(Queries.GetLastAggregateEventsSql(aggregateName));
            }
        }

        public async Task<AggregateEvent[]> GetAggregateEventsByAggregateTypeName(string aggregateName, string aggregateTypeName, long aggregateVersion)
        {
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<AggregateEvent>(Queries.GetAggregateEventsByAggregateTypeNameSql(aggregateName), new { aggregateTypeName, aggregateVersion })).ToArray();
            }
        }

        public async Task CreateEventsAndSnapshotsTables(string aggregateName)
        {
            using (IDbConnection conn = Connection)
            {
                await conn.ExecuteAsync(Queries.CreateEventsAndSnapshotsTableSql(aggregateName));
            }
        }
    }
}