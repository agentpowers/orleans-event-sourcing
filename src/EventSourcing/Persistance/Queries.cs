namespace EventSourcing.Persistance
{
    internal static class Queries
    {   
        public static string CreateEventsTableSql(string aggregateType) =>
            @"CREATE TABLE IF NOT EXISTS " + aggregateType + @"_events (
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

        public static string CreateSnapshotsTableSql(string aggregateType) => 
            @"CREATE TABLE IF NOT EXISTS " + aggregateType + @"_snapshots (
                Id bigint primary key not null generated always as identity,
                AggregateId bigint not null,
                AggregateVersion bigint not null,
                Data text,
                Created timestamp not null
            );";
        public static string CreateEventsAndSnapshotsTableSql(string aggregateType) =>
            $@"{CreateEventsTableSql(aggregateType)}
               {CreateSnapshotsTableSql(aggregateType)}";
        public static string NewAggregate(string aggregateType)
        {
            return $@"BEGIN TRANSACTION;
                        {CreateEventsAndSnapshotsTableSql(aggregateType)}
                        insert into Aggregate(Type) 
                        values (@Type) 
                        returning AggregateId;
                    COMMIT;";
        }
        public static string InsertEventSql(string aggregateType)
        {
            return $@"insert into {aggregateType}_events(AggregateId, AggregateVersion, EventVersion, ParentEventId, RootEventId, Type, Data, Created)
                        values (@AggregateId, @AggregateVersion, @EventVersion, @ParentEventId, @RootEventId, @Type, @Data, @Created)
                        returning Id;";
        }
        public static string InsertSnapShotSql(string aggregateType)
        {
            return $@"insert into {aggregateType}_snapshots(AggregateId,AggregateVersion,Data, Created)
                        values (@AggregateId, @AggregateVersion, @Data, @Created)
                        returning Id;";
        }
        public const string GetAggregateByTypeSql =
            @"select * from Aggregate
                where Type=@type limit 1;";
        public static string GetSnapshotSql(string aggregateType)
        {
            return $@"select * from {aggregateType}_snapshots
                        where AggregateId=@id
                        order by Id desc
                        limit 1;";
        }
        public static string GetSnapshotAggregateVersionSql(string aggregateType)
        {
            return $@"select AggregateVersion from {aggregateType}_snapshots
                        where AggregateId=@id
                        order by Id desc
                        limit 1;";
        }
        public static string GetEventsByAggregateIdSql(string aggregateType)
        {
            return $@"select * from {aggregateType}_events
                        where AggregateId=@id and AggregateVersion > @AggregateVersion
                        order by Id asc;";
        }
        public static string GetAggregateEventsSql(string aggregateType)
        {
            return $@"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                        from {aggregateType}_events ev
                        join aggregate ag on ag.aggregateid = ev.aggregateid
                        where ev.Id > @id
                        order by ev.Id asc;";
        }
        public static string GetLastAggregateEventsSql(string aggregateType)
        {
            return $@"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                        from {aggregateType}_events ev
                        join aggregate ag on ag.aggregateid = ev.aggregateid
                        order by ev.Id desc
                        limit 1;";
        }
        public static string GetAggregateEventsByAggregateTypeNameSql(string aggregateType)
        {
            return $@"select ev.id, ev.aggregateid, ev.aggregateversion, ev.eventversion, ev.parenteventid, ev.rooteventid, ev.type, ev.data, ev.created, ag.type as aggregateType
                        from {aggregateType}_events ev
                        join aggregate ag on ag.aggregateid = ev.aggregateid
                        where ag.type=@AggregateTypeName and ev.AggregateVersion > @AggregateVersion
                        order by ev.Id asc;";
        }   
    }
}