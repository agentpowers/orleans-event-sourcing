namespace EventSourcing.Persistance
{
    internal static class Queries
    {   
        public const string NewAggregateTableSql= 
            @"CREATE TABLE IF NOT EXISTS Aggregate (
                AggregateId bigint primary key not null generated always as identity,
                Type varchar(255) not null UNIQUE,
                Created timestamp not null
            );";
        public static string NewAggregate(string aggregateType)
        {
            return @"BEGIN TRANSACTION;
                        CREATE TABLE IF NOT EXISTS " + aggregateType + @"_events (
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
                        );
                        CREATE TABLE IF NOT EXISTS " + aggregateType + @"_snapshots (
                            Id bigint primary key not null generated always as identity,
                            AggregateId bigint not null,
                            AggregateVersion bigint not null,
                            Data text,
                            Created timestamp not null,
                        );
                        insert into Aggregate(Type) values (@Type) returning AggregateId;
                    COMMIT;";
        }
        public static string InsertEventSql(string aggregateType)
        {
            return $"insert into {aggregateType}_events(AggregateId, AggregateVersion, EventVersion, ParentEventId, RootEventId, Type, Data) values (@AggregateId, @AggregateVersion, @EventVersion, @ParentEventId, @RootEventId, @Type, @Data) returning Id";
        }
        public static string InsertSnapShotSql(string aggregateType)
        {
            return $"insert into {aggregateType}_snapshots(AggregateId,AggregateVersion,Data) values (@AggregateId, @AggregateVersion, @Data) returning Id";
        }
        public const string GetAggregateSql =
            "select * from Aggregate where AggregateId=@id and Type=@type limit 1";
        public const string GetAggregateByTypeSql =
            "select * from Aggregate where Type=@type limit 1";
        public const string GetAggregatesByTypeSql =
            "select * from Aggregate where Type=@type";
        public static string GetSnapshotSql(string aggregateType)
        {
            return $"select * from {aggregateType}_snapshots where AggregateId=@id order by Id desc limit 1";
        }
        public static string GetSnapshotAggregateVersionSql(string aggregateType)
        {
            return $"select AggregateVersion from {aggregateType}_snapshots where AggregateId=@id order by Id desc limit 1";
        }
        public static string GetEventsSql(string aggregateType)
        {
            return $"select * from {aggregateType}_events where AggregateId=@id and AggregateVersion > @AggregateVersion order by Id asc";
        }
        public static string GetLastEventsSql(string aggregateType)
        {
            return $"select * from {aggregateType}_events order AggregateId=@id by Id desc limit 1";
        }       
    }
}