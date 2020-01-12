using System;
using System.Data;
using Dapper;
using System.Threading.Tasks;
using Npgsql;
using System.Linq;

namespace Persistance
{
    public class Repository : IRepository
    {
        private IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection("host=localhost;database=EventSourcing;username=orleans;password=orleans");
            }
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

        public async Task<Event[]> GetEvents(long aggregateId, long lastEventSequence)
        {
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<Event>(Queries.GetEventsSql, new { id = aggregateId, lastEventSequence = lastEventSequence })).ToArray();
            }
        }

        public async Task<Event> GetLastEvent(long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<Event>(Queries.GetLastEventsSql, new { id = aggregateId });
            }
        }

        public async Task<(Snapshot, Event[])> GetSnapshotAndEvents(long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                var snapshot  = await conn.QueryFirstOrDefaultAsync<Snapshot>(Queries.GetSnapshotSql, new { id = aggregateId });
                return (snapshot, (await conn.QueryAsync<Event>(Queries.GetEventsSql, new { id = aggregateId, lastEventSequence = (snapshot?.LastEventSequence).GetValueOrDefault() })).ToArray());
            }
        }

        public async Task<long> GetSnapshotLastEventSequence(long aggregateId)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstAsync<long>(Queries.GetSnapshotLastEventSequenceSql, new { id = aggregateId });
            }
        }

        public async Task InitTables()
        {
            using (IDbConnection conn = Connection)
            {
                await conn.ExecuteAsync(Queries.NewAggregateTableSql);
                await conn.ExecuteAsync(Queries.NewEventsTableSql);
                await conn.ExecuteAsync(Queries.NewSnapshotsTabelSql);
            }
        }

        public async Task<long> SaveAggregate(Aggregate aggregate)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(Queries.InsertAggregateSql, aggregate);
            }

        }

        public async Task<long> SaveEvent(Event @event)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(Queries.InsertEventSql, @event);
            }
        }

        public async Task<long> SaveSnapshot(Snapshot snapshot)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(Queries.InsertSnapShotSql, snapshot);
            }
        }
    }
}