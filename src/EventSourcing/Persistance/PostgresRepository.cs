using System.Data;
using Dapper;
using System.Threading.Tasks;
using Npgsql;
using System.Linq;

namespace EventSourcing.Persistance
{
    public class PostgresRepository : IRepository
    {
        private readonly string _connectionString;
        private IDbConnection Connection => new NpgsqlConnection(_connectionString);

        public PostgresRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Aggregate> GetAggregateByTypeName(string type)
        {
            const string sql = "select * from Aggregate where Type=@type limit 1;";
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<Aggregate>(sql, new { type });
        }

        public async Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId)
        {
            const string sql = @"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                from {0}_events ev
                                join aggregate ag on ag.aggregateid = ev.aggregateid
                                where ev.Id > @id
                                order by ev.Id asc;";
            using var conn = Connection;
            return (await conn.QueryAsync<AggregateEvent>(string.Format(sql, aggregateName), new { id = afterEventId })).ToArray();
        }

        public async Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long afterEventId, int size)
        {
            const string sql = @"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                from {0}_events ev
                                join aggregate ag on ag.aggregateid = ev.aggregateid
                                where ev.Id > @id
                                order by ev.Id asc
                                limit {1};";
            using var conn = Connection;
            return (await conn.QueryAsync<AggregateEvent>(string.Format(sql, aggregateName, size), new { id = afterEventId })).ToArray();
        }

        public async Task<(Snapshot, AggregateEventBase[])> GetSnapshotAndEvents(string aggregateName, long aggregateId)
        {
            const string snapshotSql = @"select * from {0}_snapshots
                                            where AggregateId=@id
                                            order by Id desc
                                            limit 1;";
            const string getEventsSql = @"select * from {0}_events
                                            where AggregateId=@id and AggregateVersion > @AggregateVersion
                                            order by Id asc;";
            using var conn = Connection;
            var snapshot = await conn.QueryFirstOrDefaultAsync<Snapshot>(string.Format(snapshotSql, aggregateName), new { id = aggregateId });
            return (snapshot, (await conn.QueryAsync<AggregateEventBase>(string.Format(getEventsSql, aggregateName), new { id = aggregateId, aggregateVersion = (snapshot?.AggregateVersion).GetValueOrDefault() })).ToArray());
        }

        public async Task<long> SaveAggregate(Aggregate aggregate)
        {
            const string newAggregateSql = @"BEGIN TRANSACTION;
                                                insert into Aggregate(Type) 
                                                values (@Type) 
                                                returning AggregateId;
                                            COMMIT;";
            using var conn = Connection;
            return await conn.ExecuteScalarAsync<long>(newAggregateSql, aggregate);
        }

        public async Task<long> SaveEvent(string aggregateName, AggregateEventBase @event)
        {
            const string sql = @"insert into {0}_events(AggregateId, AggregateVersion, EventVersion, ParentEventId, RootEventId, Type, Data, Created)
                                values (@AggregateId, @AggregateVersion, @EventVersion, @ParentEventId, @RootEventId, @Type, @Data, @Created)
                                returning Id;";
            using var conn = Connection;
            return await conn.ExecuteScalarAsync<long>(string.Format(sql, aggregateName), @event);
        }

        public async Task<long> SaveEvents(string aggregateName, params AggregateEventBase[] events)
        {
            const string sql = @"insert into {0}_events(AggregateId, AggregateVersion, EventVersion, ParentEventId, RootEventId, Type, Data, Created)
                                values (@AggregateId, @AggregateVersion, @EventVersion, @ParentEventId, @RootEventId, @Type, @Data, @Created)
                                returning Id;";

            using var conn = Connection;
            conn.Open();
            using var trans = conn.BeginTransaction();
            long lastId = 0;
            try
            {
                foreach (var @event in events)
                {
                    lastId = await conn.ExecuteScalarAsync<long>(string.Format(sql, aggregateName), @event, trans);
                }
                trans.Commit();
            }
            catch (System.Exception)
            {
                trans.Rollback();
                throw;
            }
            return lastId;
        }

        public async Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot)
        {
            const string sql = @"insert into {0}_snapshots(AggregateId,AggregateVersion,Data, Created)
                                values (@AggregateId, @AggregateVersion, @Data, @Created)
                                returning Id;";
            using var conn = Connection;
            return await conn.ExecuteAsync(string.Format(sql, aggregateName), snapshot);
        }

        public async Task<AggregateEvent> GetLastAggregateEvent(string aggregateName)
        {
            const string sql = @"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                    from {0}_events ev
                    join aggregate ag on ag.aggregateid = ev.aggregateid
                    order by ev.Id desc
                    limit 1;";
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<AggregateEvent>(string.Format(sql, aggregateName));
        }

        public async Task<AggregateEvent[]> GetAggregateEventsByAggregateTypeName(string aggregateName, string aggregateTypeName, long aggregateVersion)
        {
            const string str = @"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                from {0}_events ev
                                join aggregate ag on ag.aggregateid = ev.aggregateid
                                where ag.type=@AggregateTypeName and ev.AggregateVersion > @AggregateVersion
                                order by ev.Id asc;";
            using var conn = Connection;
            return (await conn.QueryAsync<AggregateEvent>(string.Format(str, aggregateName), new { aggregateTypeName, aggregateVersion })).ToArray();
        }

        public async Task CreateEventsAndSnapshotsTables(string aggregateName)
        {
            const string createEventsTableSql = @"CREATE TABLE IF NOT EXISTS {0}_events (
                                                Id bigint primary key not null generated always as identity,
                                                AggregateId bigint not null,
                                                AggregateVersion bigint not null,
                                                EventVersion int not null,
                                                ParentEventId bigint null,
                                                RootEventId bigint null,
                                                Type varchar(255) not null,
                                                Data text,
                                                Created timestamp not null,
                                                UNIQUE (AggregateId, AggregateVersion)
                                            );";
            const string createSnapshotTableSql = @"CREATE TABLE IF NOT EXISTS {0}_snapshots (
                    Id bigint primary key not null generated always as identity,
                    AggregateId bigint not null,
                    AggregateVersion bigint not null,
                    Data text,
                    Created timestamp not null
                );";
            const string sql = @"BEGIN TRANSACTION;
                                SELECT pg_advisory_xact_lock({0});
                                {1}
                                {2}
                            COMMIT;
                            ";
            using var conn = Connection;
            await conn.ExecuteAsync(string.Format(
                sql,
                aggregateName.GetHashCode(),
                string.Format(createEventsTableSql, aggregateName),
                string.Format(createSnapshotTableSql, aggregateName))
            );
        }
    }
}