namespace EventSourcing.Persistance
{
    internal static class Queries
    {

        public const string NewAggregateTableSql= 
            @"CREATE TABLE IF NOT EXISTS Aggregate (
                AggregateId bigint primary key not null generated always as identity,
                Type varchar(255) not null UNIQUE,
                Created timestamp default current_timestamp
            );";

        public const string NewEventsTableSql= 
            @"CREATE TABLE IF NOT EXISTS Events (
                Sequence bigint primary key not null generated always as identity,
                AggregateId bigint not null,
                RowVersion bigint not null,
                Type varchar(255) not null,
                Data text,
                Created timestamp default current_timestamp
            );";
        public const string NewSnapshotsTabelSql= 
            @"CREATE TABLE IF NOT EXISTS Snapshots (
                Sequence bigint primary key not null generated always as identity,
                AggregateId bigint not null,
                LastEventSequence bigint not null,
                Data text,
                Created timestamp default current_timestamp
            );";
        public const string InsertAggregateSql=
            "insert into Aggregate(Type) values (@Type) returning AggregateId";

        public const string InsertEventSql=
            "insert into Events(AggregateId,RowVersion, Type,Data) values (@AggregateId,@RowVersion,@Type,@Data) returning Sequence";

        public const string InsertSnapShotSql=
            "insert into Snapshots(AggregateId,LastEventSequence,Data) values (@AggregateId,@LastEventSequence,@Data) returning Sequence";

        public const string GetAggregateSql =
            "select * from Aggregate where AggregateId=@id and Type=@type limit 1";

        public const string GetAggregateByTypeSql =
            "select * from Aggregate where Type=@type limit 1";

        public const string GetAggregatesByTypeSql =
            "select * from Aggregate where Type=@type";

        public const string GetSnapshotSql =
            "select * from Snapshots where AggregateId=@id order by Sequence desc limit 1";

        public const string GetSnapshotLastEventSequenceSql =
            "select LastEventSequence from Snapshots where AggregateId=@id order by Sequence desc limit 1";

        public const string GetEventsSql =
            "select * from Events where AggregateId=@id and Sequence > @lastEventSequence order by Sequence";

        public const string GetLastEventsSql =
            "select * from Events where AggregateId=@id order by Sequence desc limit 1";
    }
}