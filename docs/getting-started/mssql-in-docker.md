# Running SQL Server in Docker

Unit tests run against an in-memory SQLite database and need no setup. The
integration and functional tests, however — and the [`TardisBank`](../../Src/Samples/TardisBank)
sample — expect a real SQL Server. The easiest way to get one locally is the
Docker script shipped in the [`/Docker`](../../Docker) folder.

## Quick start

With [Docker](https://docs.docker.com/get-docker/) running, from the repository root:

```bash
pwsh Docker/start-mssql.ps1
```

This starts a SQL Server 2019 container in the background and returns
immediately. What it sets up:

| Setting | Value |
|---|---|
| Image | `mcr.microsoft.com/mssql/server:2019-latest` |
| Container name | `sharparch-sql` |
| Port | **2433** (host) → 1433 (container) |
| Credentials | `sa` / `Password12!` |
| Data | persisted in `Docker/mssql-data/` |

The port is `2433` rather than the default `1433` so the container doesn't
clash with a locally installed SQL Server default instance.

The script is a thin wrapper; the equivalent raw command is:

```bash
docker run --name sharparch-sql --user root \
    -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Password12!' \
    -p 2433:1433 -d \
    -v ./mssql-data:/var/opt/mssql/data \
    mcr.microsoft.com/mssql/server:2019-latest
```

## Create the TardisBank database

The sample expects a database named `TardisBank`. Unlike the PostgreSQL
container, the SQL Server container does not create it automatically — create
it once with `sqlcmd` inside the container:

```bash
docker exec -it sharparch-sql /opt/mssql-tools/bin/sqlcmd -U sa -P 'Password12!' \
    -Q "CREATE DATABASE TardisBank"
```

[`Docker/create_database.sql`](../../Docker/create_database.sql) contains the
equivalent T-SQL with explicit file/filegroup settings, if you prefer to run it
from SSMS or Azure Data Studio.

## Connect to it

This is the connection string the TardisBank sample uses
(`Src/Samples/TardisBank/Src/Suteki.TardisBank.WebApi/NHibernate.config`) —
note the **port 2433** after the server name:

```
Data Source=localhost,2433;Initial Catalog=TardisBank;Integrated Security=False;User ID=sa;Password=Password12!;Application Name=Suteki.TardisBank;Encrypt=false;
```

## Managing the container

```bash
docker stop sharparch-sql    # stop the server
docker start sharparch-sql   # start it again — data is kept
docker rm -f sharparch-sql   # remove the container — data is kept in Docker/mssql-data
```

To wipe the data completely, also delete the `Docker/mssql-data/` folder (while
the container is stopped).

## Notes

* The `sa` password above is for local development only — never use it anywhere else.
* CI (AppVeyor) does not use this container; it runs SQL Server as a build
  service configured by [`mssql-setup.ps1`](../../mssql-setup.ps1) at the repository root.
* A PostgreSQL equivalent is available: `pwsh Docker/start-postgres.ps1` (port 5432,
  same password, and it creates the `TardisBank` database automatically on first
  start via `create_database_postgres.sql`).
