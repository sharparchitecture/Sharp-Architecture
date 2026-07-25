docker run --name sharparch-postgres `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=Password12! `
  -p 5432:5432 `
  -v ${PWD}/postgres-data:/var/lib/postgresql `
  -v ${PWD}/create_database_postgres.sql:/docker-entrypoint-initdb.d/create_database_postgres.sql `
  -d postgres:latest
