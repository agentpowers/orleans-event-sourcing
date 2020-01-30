FROM postgres:latest
COPY sql-scripts /docker-entrypoint-initdb.d/