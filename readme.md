# k8s (using skaffold)
run  ```skaffold run``` or ```skaffold dev``` from src folder

## In k8s access psql by port forwarding postgres port 5432 to local 5432 port.  Then use gui like azure data studio to access the db.
```
kubectl port-forward pods/postgres-demo-0 5432:5432 -n default
```
## ORLEANS DASHBOARD - http://localhost:5000/dashboard

## Account Endpoints

http://localhost:5000/account/1

http://localhost:5000/account/1/deposit?amount=100

http://localhost:5000/account/1/withdraw?amount=100



# Runnig locally
## set environment variable "ORLEANS_ENV" with value "LOCAL" - this is needed to switch orlean cluster config and database connection string to use local values.  If debugging using VSCODE this variable is already configured in .vscode/launch.json
## create a postgres database with name "EventSourcing". Create username "orleans" with password "orleans" with access to "EventSourcing" database
## In local database run the following script to add Aggregate table
```
CREATE TABLE IF NOT EXISTS Aggregate (
    AggregateId bigint primary key not null generated always as identity,
    Type varchar(255) not null UNIQUE,
    Created timestamp default current_timestamp
);
```
## start application
```
cd src/API && dotnet run
```
OR 
Debug via VSCODE


## ORLEANS DASHBOARD - http://localhost/api/dashboard

## Account Endpoints

http://localhost/api/account/1

http://localhost/api/account/1/deposit?amount=100

http://localhost/api/account/1/withdraw?amount=100
