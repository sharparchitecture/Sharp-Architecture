-- PostgreSQL equivalent of create_database.sql
-- Compatible with the official postgres Docker Linux image
--
-- Usage options:
--   1. Place in /docker-entrypoint-initdb.d/ for automatic execution on first container start
--   2. Run manually: psql -U postgres -f create_database_postgres.sql

-- Create TardisBank database if it does not already exist
SELECT 'CREATE DATABASE "TardisBank"'
WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'TardisBank'
)\gexec
