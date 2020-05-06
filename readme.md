
# Orleans Event Sourcing

## Deploy k8s (using [skaffold](https://skaffold.dev/))

run  `skaffold run` or `skaffold dev` from src folder

### Access Postgres in k8s cluster

1. In k8s access psql by port forwarding postgres port 5432 to local 5432 port.

2. Then use any postgres database GUI like azure data studio to access the db.
```kubectl port-forward pods/postgres-demo-0 5432:5432 -n default```
3. Orleans Dashboard - http://localhost/api/dashboard

## Running locally

1. Set environment variable "ORLEANS_ENV" with value "LOCAL" - this is needed to switch orlean cluster config and database connection string to use local values.
    > If debugging using VSCODE this variable is already configured in .vscode/launch.json

2. Create a postgres database with name "EventSourcing".

3. Create username "orleans" with password "orleans" with access to "EventSourcing" database
4. In local database run script(s) in the following directory  
`src/sql-scripts`
5. start application  
`cd src/API && dotnet run`
6. Orleans Dashboard - http://localhost:5000/dashboard

### OR

#### Debug via VSCODE

## Configure POSTMAN

* import globals.json for global variables
* import k8s.environment.json for k8s variables
* import local.environment.json for local variables
* import ORLEANS API.collection for endpoints
