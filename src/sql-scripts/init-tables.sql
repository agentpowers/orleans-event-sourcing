CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);

CREATE TABLE IF NOT EXISTS Account (
    Id bigint primary key,
    Version bigint not null,
    Balance decimal not null,
    Modified timestamp not null
);
