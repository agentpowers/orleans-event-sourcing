namespace EventSourcing.Persistance
{
    internal static class DynamicQueries
    {   
        public static string NewAggregate(string aggregateType)
        {
            return @"BEGIN TRANSACTION;
                        CREATE TABLE" + aggregateType + @"_event_stream (
                            Sequence bigint primary key not null,
                            Type varchar(255) not null,
                            Data text,
                            Created timestamp default current_timestamp
                        );
                        CREATE " + aggregateType + @"_snapshots (
                            Sequence bigint primary key not null generated always as identity,
                            AggregateId bigint not null,
                            LastEventSequence bigint not null,
                            Data text,
                            Created timestamp default current_timestamp
                        );
                        insert into Aggregate(Type) values (@Type) returning AggregateId;
                    COMMIT;";
        }

        public static string InsertEventSql(string aggregateType)
        {
            return $"insert into {aggregateType}_events(Sequence, Type, Data) values (@Sequence, @Type, @Data)";
        }
        public static string InsertSnapShotSql(string aggregateType)
        {
            return $"insert into {aggregateType}_snapshots(AggregateId,LastEventSequence,Data) values (@AggregateId,@LastEventSequence,@Data) returning Sequence";
        }
        public static string GetSnapshotSql(string aggregateType)
        {
            return $"select * from {aggregateType}_snapshots order by Sequence desc limit 1";
        }
        public static string GetSnapshotLastEventSequenceSql(string aggregateType)
        {
            return $"select LastEventSequence from {aggregateType}_snapshots order by Sequence desc limit 1";
        }
        public static string GetEventsSql(string aggregateType)
        {
            return $"select * from {aggregateType}_events where Sequence > @lastEventSequence order by Sequence";
        }
        public static string GetLastEventsSql(string aggregateType)
        {
            return $"select * from {aggregateType}_events order by Sequence desc limit 1";
        }       
    }
}