init:
	docker network create cache_net
	docker build --tag pg -f src/docker/Cache.Postgres.Dockerfile src
	docker build --tag cache_app -f src/docker/Cache.Dockerfile src

up_cache_db:
	docker run --network cache_net -v /tmp/ev_cache:/var/lib/postgresql/data -e POSTGRES_DB=postgresdb -e POSTGRES_USER=postgresadmin -e POSTGRES_PASSWORD=postgrespwd -p 5432:5432 --detach --name ev_cache pg

up_cache1:
	docker run --network cache_net -e SILO_PORT=11111 -e GATEWAY_PORT=30000 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8001 -e POSTGRES_SERVICE_HOST=ev_cache -p 8001:8001 -p 11111:11111 -p 30000:30000 --detach --name cache1 cache_app

up_cache2:
	docker run --network cache_net -e SILO_PORT=11112 -e GATEWAY_PORT=30001 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8002 -e POSTGRES_SERVICE_HOST=ev_cache -p 8002:8002 -p 11112:11112 -p 30001:30001 --detach --name cache2 cache_app

down_cache_db:
	docker kill ev_cache

down_cache1:
	docker kill cache1

down_cache2:
	docker kill cache2

cleanup:
	docker rm cache1
	docker rm cache2
	docker rm ev_cache
	docker network rm cache_net