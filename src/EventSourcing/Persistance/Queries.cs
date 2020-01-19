namespace EventSourcing.Persistance
{
    internal static class Queries
    {

        public const string NewAggregateTableSql= 
            @"CREATE TABLE IF NOT EXISTS Aggregate (
                AggregateId bigint primary key not null generated always as identity,
                Version integer not null DEFAULT 0,
                Type varchar(255) not null,
                Created timestamp default current_timestamp
            );";

        public const string NewEventsTableSql= 
            @"CREATE TABLE IF NOT EXISTS Events (
                Sequence bigint primary key not null generated always as identity,
                AggregateId bigint not null,
                Version integer not null DEFAULT 0,
                Type varchar(255) not null,
                Data text,
                Created timestamp default current_timestamp
            );";
        public const string NewSnapshotsTabelSql= 
            @"CREATE TABLE IF NOT EXISTS Snapshots (
                Sequence bigint primary key not null generated always as identity,
                AggregateId bigint not null,
                Version integer not null DEFAULT 0,
                LastEventSequence bigint not null,
                Data text,
                Created timestamp default current_timestamp
            );";
        public const string InsertAggregateSql=
            "insert into Aggregate(Version,Type) values (@Version,@Type) returning AggregateId";

        public const string InsertEventSql=
            "insert into Events(AggregateId,Version,Type,Data) values (@AggregateId,@Version,@Type,@Data) returning Sequence";

        public const string InsertSnapShotSql=
            "insert into Snapshots(AggregateId,Version,LastEventSequence,Data) values (@AggregateId,@Version,@LastEventSequence,@Data) returning Sequence";

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