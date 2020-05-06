# k8s (using [skaffold](https://skaffold.dev/))
run  ```skaffold run``` or ```skaffold dev``` from src folder

## In k8s access psql by port forwarding postgres port 5432 to local 5432 port.  Then use any postgres database GUI like azure data studio to access the db.
```
kubectl port-forward pods/postgres-demo-0 5432:5432 -n default
```
## ORLEANS DASHBOARD - http://localhost/api/dashboard


# Running locally
### set environment variable "ORLEANS_ENV" with value "LOCAL" - this is needed to switch orlean cluster config and database connection string to use local values.  
>If debugging using VSCODE this variable is already configured in .vscode/launch.json
### create a postgres database with name "EventSourcing". Create username "orleans" with password "orleans" with access to "EventSourcing" database
### In local database run script(s) in the following directory  
src/sql-scripts/
### start application
```
cd src/API && dotnet run
```
OR 
Debug via VSCODE

## ORLEANS DASHBOARD - http://localhost:5000/dashboard

# POSTMAN
### import globals.json for global variables
### import k8s.environment.json for k8s variables
### import local.environment.json for local variables
### import ORLEANS API.collection for endpoints
