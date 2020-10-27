FROM postgres:latest
EXPOSE 5432
# copy Eventsourcing scripts
COPY EventSourcing/sql-scripts /docker-entrypoint-initdb.d/
# copy Account scripts
COPY examples/Account/sql-scripts /docker-entrypoint-initdb.d/

RUN ls /docker-entrypoint-initdb.d/