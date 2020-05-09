CREATE TABLE IF NOT EXISTS Account (
    Id bigint primary key,
    Version bigint not null,
    Balance decimal not null,
    Modified timestamp not null
);
