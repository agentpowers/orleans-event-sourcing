FROM postgres:latest
EXPOSE 5432
# copy Eventsourcing scripts
COPY EventSourcing/sql-scripts /docker-entrypoint-initdb.d/
# copy Cache scripts
COPY examples/SagaExample/sql-scripts /docker-entrypoint-initdb.d/