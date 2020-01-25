
# DEPLOYING CLUSTER
skaffold run or skaffold dev

# INIT POSTGRES DB
## Access psql by exec into postgredb POD in kubernetes.  Then run the following to connect psql
```
psql -h localhost -U postgresadmin -p 5432 postgresdb -W
```

## Then run the following 3 scripts to add tables
```
CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);
```

```
CREATE TABLE IF NOT EXISTS Events (
    Sequence bigint primary key not null generated always as identity,
    AggregateId bigint not null,
    Version bigint not null DEFAULT 0,
    Type varchar(255) not null,
    Data text,
    Created timestamp default current_timestamp
);
```
```
CREATE TABLE IF NOT EXISTS Snapshots (
    Sequence bigint primary key not null generated always as identity,
    AggregateId bigint not null,
    LastEventSequence bigint not null,
    Data text,
    Created timestamp default current_timestamp
);
```

# TESTING ENDPOINTS
http://localhost/api/account/1
http://localhost/api/account/1/deposit?amount=100
http://localhost/api/account/1/withdraw?amount=100
