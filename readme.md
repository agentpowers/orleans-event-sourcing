
# DEPLOYING CLUSTER
skaffold run or skaffold dev

# INIT POSTGRES DB
## In k8s access psql by port forwarding postgres port 5432 to local 5432 port.  Then use gui like azure data studio to access the db.
```
kubectl port-forward pods/postgres-demo-0 5432:5432 -n default
```
### local db credentials -> host=localhost;database=EventSourcing;username=orleans;password=orleans
### k8s db credentials -> host=localhot;database=postgresdb;username=postgresadmin;password=postgrespwd

## In local database run the following script to add Aggregate table.  In K8s this is initialized using src/sql-scripts
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
