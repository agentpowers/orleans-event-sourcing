CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);

CREATE TABLE IF NOT EXISTS Events (
    Sequence bigint primary key not null generated always as identity,
    AggregateId bigint not null,
    Version bigint not null DEFAULT 0,
    Type varchar(255) not null,
    Data text,
    Created timestamp default current_timestamp
);
CREATE TABLE IF NOT EXISTS Snapshots (
    Sequence bigint primary key not null generated always as identity,
    AggregateId bigint not null,
    LastEventSequence bigint not null,
    Data text,
    Created timestamp default current_timestamp
);