CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);

-- CREATE TABLE Aggregate (
--     AggregateId bigint primary key not null identity,
--     Type varchar(255) not null UNIQUE,
--     Created datetime not null default GETDATE()
-- );
