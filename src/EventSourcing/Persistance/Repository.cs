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

        public async Task<Event[]> GetEvents(string aggregateType, long lastEventSequence)
        {
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<Event>(DynamicQueries.GetEventsSql(aggregateType), new { lastEventSequence = lastEventSequence })).ToArray();
            }
        }

        public async Task<Event> GetLastEvent(string aggregateType)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<Event>(DynamicQueries.GetLastEventsSql(aggregateType));
            }
        }

        public async Task<(Snapshot, Event[])> GetSnapshotAndEvents(string aggregateType)
        {
            using (IDbConnection conn = Connection)
            {
                var snapshot  = await conn.QueryFirstOrDefaultAsync<Snapshot>(DynamicQueries.GetSnapshotSql(aggregateType));
                return (snapshot, (await conn.QueryAsync<Event>(DynamicQueries.GetEventsSql(aggregateType), new { lastEventSequence = (snapshot?.LastEventSequence).GetValueOrDefault() })).ToArray());
            }
        }

        public async Task<long> GetSnapshotLastEventSequence(string aggregateType)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<long>(DynamicQueries.GetSnapshotLastEventSequenceSql(aggregateType));
            }
        }

        public async Task<long> SaveAggregate(Aggregate aggregate)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(DynamicQueries.NewAggregate(aggregate.Type), aggregate);
            }
        }

        public async Task<long> SaveEvent(string aggregateType, Event @event)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(DynamicQueries.InsertEventSql(aggregateType), @event);
            }
        }

        public async Task<long> SaveSnapshot(string aggregateType, Snapshot snapshot)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(DynamicQueries.InsertSnapShotSql(aggregateType), snapshot);
            }
        }
    }
}