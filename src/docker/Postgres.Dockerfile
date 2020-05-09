FROM postgres:latest
# copy Eventsourcing scripts
COPY EventSourcing/sql-scripts /docker-entrypoint-initdb.d/
# copy Account scripts
COPY examples/Account/sql-scripts /docker-entrypoint-initdb.d/