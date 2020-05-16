
# Event Sourcing with [Orleans](https://github.com/dotnet/orleans)

## Running locally

1. Set environment variable "ORLEANS_ENV" with value "LOCAL" - this is needed to switch orlean cluster config and database connection string to use local values.
    > If debugging using VSCODE this variable is already configured in .vscode/launch.json

2. Create a postgres database with name "EventSourcing".
    * `CREATE DATABASE EventSourcing;`
3. Create username "orleans" with password "orleans" with access to "EventSourcing" database
    * `CREATE USER orleans WITH PASSWORD 'orleans';`
    * `GRANT ALL PRIVILEGES ON DATABASE EventSourcing to orleans;`
4. In local database run script(s) in the following directories
    * `src/EventSourcing/sql-scripts`
    * `src/examples/Account/sql-scripts`
5. start application  
`cd src/examples/Account && dotnet run`
6. Orleans Dashboard is available at - http://localhost:5000/dashboard

### OR

#### Debug via VSCODE

1. configured to debug examples/account

## Deploy to k8s (using [skaffold](https://skaffold.dev/))

1. run  `cd src/k8s && skaffold run`
2. Orleans Dashboard is available at - http://localhost/api/dashboard

### Access Postgres in k8s cluster

1. In k8s access psql by port forwarding postgres port 5432 to local 5432 port.

2. Then use any postgres database GUI like azure data studio to access the db.
```kubectl port-forward pods/postgres-demo-0 5432:5432 -n default```
    * database: postgresdb
    * user: postgresadmin
    * password: postgrespwd


## Configure POSTMAN

* import globals.json for global variables
* import k8s.environment.json for k8s variables
* import local.environment.json for local variables
* import ORLEANS API.collection for endpoints
