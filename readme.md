
# Event Sourcing with [Orleans](https://github.com/dotnet/orleans)

## Running locally using docker

1. Run account sample locally
    * Create a docker network for pod communication and build both postgres and account containers
        * `make build_account`
    * Starts a localhost postgres server in docker with following config `host=localhost;database=postgresdb;username=postgresadmin;password=postgrespwd`.  This db is persisted in local /tmp/ev_account directory.  Delete this folder if a clean db is needed.
        * `make up_account_db`
    * Starts account app with port 8001 (ex: dashboard url http://localhost:8001/dashborad)
        * `make up_account1`

    * Optionally start second instance of account
        * `make up_account2`
            * starts account app with port 8002 (ex: dashboard url http://localhost:8002/dashborad)

    * To cleanup above steps
        * `make cleanup_account`

## Running locally

1. Set environment variable "ORLEANS_ENV" with value "LOCAL" - this is needed to switch orleans cluster config and database connection string to use local values.
    > If debugging using VSCODE this variable is already configured in .vscode/launch.json

2. Create db using docker or manually
    * ### Using docker 
        * `make run_account_db`
        * To cleanup above
            * `make cleanup_account_db`
    * ### Create database manually
        * Create a postgres database with name "postgresdb".
            * `CREATE DATABASE postgresdb;`
        * Create username "postgresadmin" with password "postgrespwd" and give access to "postgresdb" database
            * `CREATE USER postgresadmin WITH PASSWORD 'postgrespwd';`
            * `GRANT ALL PRIVILEGES ON DATABASE postgresdb to postgresadmin;`
        * In local database run script(s) in the following directories
            * `src/EventSourcing/sql-scripts`
            * `src/examples/Account/sql-scripts`

5. Start application  
    * `cd src/examples/Account && dotnet run`

6. Orleans Dashboard is available at - http://localhost:5000/dashboard

## Debug via VSCODE

1. Follow Running locally steps 1-2
2. Start Debugging in vscode by launching __Launch account app__.  This is configured to debug examples/account

## Deploy to k8s (using [skaffold](https://skaffold.dev/))

1. Run  `cd src/k8s && skaffold run`
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

### to run postman runners in parallel see postman/parallel_runner directory
