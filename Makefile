#cache sample
init_cache:
	docker network create cache_net
	docker build --tag cache_db -f src/docker/Cache.Postgres.Dockerfile src
	docker build --tag cache_app -f src/docker/Cache.Dockerfile src

up_cache_db:
	docker run --network cache_net -v /tmp/ev_cache:/var/lib/postgresql/data -e POSTGRES_DB=postgresdb -e POSTGRES_USER=postgresadmin -e POSTGRES_PASSWORD=postgrespwd -p 5432:5432 --detach --name ev_cache cache_db

up_cache1:
	docker run --network cache_net -e SILO_PORT=11111 -e GATEWAY_PORT=30000 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8001 -e POSTGRES_SERVICE_HOST=ev_cache -p 8001:8001 -p 11111:11111 -p 30000:30000 --detach --name cache1 cache_app

up_cache2:
	docker run --network cache_net -e SILO_PORT=11112 -e GATEWAY_PORT=30001 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8002 -e POSTGRES_SERVICE_HOST=ev_cache -p 8002:8002 -p 11112:11112 -p 30001:30001 --detach --name cache2 cache_app

down_cache_db:
	docker kill ev_cache

down_cache1:
	docker kill cache1
	docker rm cache1

down_cache2:
	docker kill cache2
	docker rm cache2

cleanup_cache:
	docker kill ev_cache; \
	docker kill cache1; \
	docker kill cache2; \
	docker rm cache1; \
	docker rm cache2; \
	docker rm ev_cache; \
	docker network rm cache_net;

#account sample
build_account:
	docker network create account_net
	docker build --tag account_db -f src/docker/Account.Postgres.Dockerfile src
	docker build --tag account_app -f src/docker/Account.Dockerfile src

up_account_db:
	docker run --network account_net -v /tmp/ev_account:/var/lib/postgresql/data -e POSTGRES_DB=postgresdb -e POSTGRES_USER=postgresadmin -e POSTGRES_PASSWORD=postgrespwd -p 5432:5432 --detach --name ev_account account_db

up_account1:
	docker run --network account_net -e SILO_PORT=11111 -e GATEWAY_PORT=30000 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8001 -e POSTGRES_SERVICE_HOST=ev_account -p 8001:8001 -p 11111:11111 -p 30000:30000 --detach --name account1 account_app

up_account2:
	docker run --network account_net -e SILO_PORT=11112 -e GATEWAY_PORT=30001 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8002 -e POSTGRES_SERVICE_HOST=ev_account -p 8002:8002 -p 11112:11112 -p 30001:30001 --detach --name account2 account_app

up_account3:
	docker run --network account_net -e SILO_PORT=11112 -e GATEWAY_PORT=30002 -e ORLEANS_ENV=LOCAL -e CUSTOM_PORT=8003 -e POSTGRES_SERVICE_HOST=ev_account -p 8003:8003 -p 11113:11113 -p 30002:30002 --detach --name account3 account_app

down_account_db:
	docker kill ev_account

down_account1:
	docker kill account1
	docker rm account1

down_account2:
	docker kill account2
	docker rm account2

cleanup_account:
	docker kill account1; \
	docker kill account2; \
	docker kill account2; \
	docker kill ev_account; \
	docker rm account1; \
	docker rm account2; \
	docker rm account3; \
	docker rm ev_account; \
	docker network rm account_net;

run_account_db:
	docker build --tag account_db -f src/docker/Account.Postgres.Dockerfile src
	docker run -v /tmp/ev_account:/var/lib/postgresql/data -e POSTGRES_DB=postgresdb -e POSTGRES_USER=postgresadmin -e POSTGRES_PASSWORD=postgrespwd -p 5432:5432 --detach --name ev_account account_db

cleanup_account_db:
	docker kill ev_account; \
	docker rm ev_account; \

# deploy to k8s
k8s_run_account:
	cd src/k8s && skaffold run

# delete from k8s
k8s_delete_account:
	cd src/k8s && skaffold delete

