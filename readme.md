
# DEPLOYING CLUSTER
skaffold run or skaffold dev

# INIT POSTGRES DB
## Access psql by exec into postgredb POD in kubernetes.  Then run the following to connect psql
```
psql -h localhost -U postgresadmin -p 5432 postgresdb -W
```

## In local database run the following script to add Aggregate table.  In K8s this is initialized using sql-scripts
```
CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);
```

# TESTING ENDPOINTS
http://localhost/api/account/1

http://localhost/api/account/1/deposit?amount=100

http://localhost/api/account/1/withdraw?amount=100
