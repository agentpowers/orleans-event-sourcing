using System.Data;
using Dapper;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace EventSourcing.Persistance
{
    public class SqlServerRepository : IRepository
    {
        private string _connectionString;
        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public SqlServerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Aggregate> GetAggregateByTypeName(string type)
        {
            const string sql = @"BEGIN TRANSACTION;
                                    select top 1 * from Aggregate where Type=@type;
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<Aggregate>(sql, new { type });
            }
        }

        public async Task<AggregateEvent[]> GetAggregateEvents(string aggregateName, long eventId)
        {
            const string sql = @"BEGIN TRANSACTION;
                                    select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                    from {0}_events ev
                                    join aggregate ag on ag.aggregateid = ev.aggregateid
                                    where ev.Id > @id
                                    order by ev.Id asc;
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<AggregateEvent>(string.Format(sql, aggregateName), new { id = eventId })).ToArray();
            }
        }

        public async Task<(Snapshot, AggregateEventBase[])> GetSnapshotAndEvents(string aggregateName, long aggregateId)
        {
            const string snapshotSql = @"BEGIN TRANSACTION;
                                            select top 1 * from {0}_snapshots
                                            where AggregateId=@id
                                            order by Id desc;
                                        COMMIT;";
            const string getEventsSql = @"BEGIN TRANSACTION;
                                            select * from {0}_events
                                            where AggregateId=@id and AggregateVersion > @AggregateVersion
                                            order by Id asc;
                                        COMMIT;";
            using (IDbConnection conn = Connection)
            {
                var snapshot  = await conn.QueryFirstOrDefaultAsync<Snapshot>(string.Format(snapshotSql, aggregateName), new { id = aggregateId });
                return (snapshot, (await conn.QueryAsync<AggregateEventBase>(string.Format(getEventsSql, aggregateName), new { id = aggregateId, aggregateVersion = (snapshot?.AggregateVersion).GetValueOrDefault() })).ToArray());
            }
        }

        public async Task<long> SaveAggregate(Aggregate aggregate)
        {
            const string newAggregateSql = @"BEGIN TRANSACTION;
                                                insert into Aggregate(Type) 
                                                values (@Type);
                                                SELECT SCOPE_IDENTITY();
                                            COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(newAggregateSql, aggregate);
            }
        }

        public async Task<long> SaveEvent(string aggregateName, AggregateEventBase @event)
        {
            const string sql = @"BEGIN TRANSACTION;
                                    insert into {0}_events(AggregateId, AggregateVersion, EventVersion, ParentEventId, RootEventId, Type, Data, Created)
                                    values (@AggregateId, @AggregateVersion, @EventVersion, @ParentEventId, @RootEventId, @Type, @Data, @Created);
                                    SELECT SCOPE_IDENTITY();
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteScalarAsync<long>(string.Format(sql, aggregateName), @event);
            }
        }

        public async Task<long> SaveSnapshot(string aggregateName, Snapshot snapshot)
        {
            const string sql = @"BEGIN TRANSACTION;
                                    insert into {0}_snapshots(AggregateId,AggregateVersion,Data, Created)
                                    values (@AggregateId, @AggregateVersion, @Data, @Created);
                                    SELECT SCOPE_IDENTITY();
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return await conn.ExecuteAsync(string.Format(sql, aggregateName), snapshot);
            }
        }

        public async Task<AggregateEvent> GetLastAggregateEvent(string aggregateName)
        {
            const string sql = @"BEGIN TRANSACTION;
                                    select top 1 ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                    from {0}_events ev
                                    join aggregate ag on ag.aggregateid = ev.aggregateid
                                    order by ev.Id desc;
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<AggregateEvent>(string.Format(sql, aggregateName));
            }
        }

        public async Task<AggregateEvent[]> GetAggregateEventsByAggregateTypeName(string aggregateName, string aggregateTypeName, long aggregateVersion)
        {
            const string str = @"BEGIN TRANSACTION;
                                    select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                                    from {0}_events ev
                                    join aggregate ag on ag.aggregateid = ev.aggregateid
                                    where ag.type=@AggregateTypeName and ev.AggregateVersion > @AggregateVersion
                                    order by ev.Id asc;
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                return (await conn.QueryAsync<AggregateEvent>(string.Format(str, aggregateName), new { aggregateTypeName, aggregateVersion })).ToArray();
            }
        }

        public async Task CreateEventsAndSnapshotsTables(string aggregateName)
        {
            const string createEventsTableSql = @"
                IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = '{0}_events'))
                BEGIN
                    CREATE TABLE {0}_events(
                        Id [bigint] IDENTITY(1,1) NOT NULL,
                        AggregateId bigint not null,
                        AggregateVersion bigint not null,
                        EventVersion int not null,
                        ParentEventId bigint null,
                        RootEventId bigint null,
                        Type varchar(255) not null,
                        Data nvarchar(max),
                        Created datetime not null default GETDATE(),
                        CONSTRAINT [PK_{0}_events] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC
                        ),
                        CONSTRAINT [UQ_{0}_events_aggregateId_aggregateversion] UNIQUE NONCLUSTERED
                        (
                            AggregateId, AggregateVersion
                        )
                    ) ON [PRIMARY]
                END";
            const string createSnapshotTableSql = @"
                IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = '{0}_snapshots'))
                BEGIN
                    CREATE TABLE {0}_snapshots(
                        Id [bigint] IDENTITY(1,1) NOT NULL,
                        AggregateId bigint not null,
                        AggregateVersion bigint not null,
                        Data nvarchar(max),
                        Created datetime not null default GETDATE(),
                        CONSTRAINT [PK_{0}_snapshots] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC
                        )
                    ) ON [PRIMARY]
                END";
            const string sql = @"BEGIN TRANSACTION;
                                {0}
                                {1}
                                COMMIT;";
            using (IDbConnection conn = Connection)
            {
                await conn.ExecuteAsync(string.Format(
                    sql,
                    string.Format(createEventsTableSql, aggregateName),
                    string.Format(createSnapshotTableSql, aggregateName))
                );
            }
        }
    }
}