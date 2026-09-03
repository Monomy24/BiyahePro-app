#!/bin/bash

set -e

echo "=========================================="
echo "Creating BiyahePro application database role"
echo "=========================================="

# NOTE: the previous version of this script wrapped the CREATE/ALTER ROLE
# logic in a `DO $do$ ... $do$;` block and tried to use psql's `:'var'`
# substitution *inside* it. psql does not substitute variables inside
# dollar-quoted strings (by design, so it doesn't corrupt the quoted
# body) — so `:'app_db_user'` was sent to Postgres literally, which is
# not valid SQL and always failed with a syntax error, meaning the role
# was never actually created.
#
# Fix: do the existence check and the substitution at the top level
# (outside any dollar-quoting) using psql's \gset / \if meta-commands.
# :"app_db_user" (double-quoted) substitutes as a safely-quoted
# identifier; :'app_db_password' (single-quoted) substitutes as a
# safely-escaped string literal. Both work correctly here because
# nothing is inside a dollar-quoted block.
psql \
  -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  -v app_db_user="$APP_DB_USER" \
  -v app_db_password="$APP_DB_PASSWORD" <<'EOSQL'

SELECT EXISTS (
    SELECT FROM pg_roles WHERE rolname = :'app_db_user'
) AS role_exists \gset

\if :role_exists
    ALTER ROLE :"app_db_user" WITH PASSWORD :'app_db_password';
    \echo 'Updated password for application role'
\else
    CREATE ROLE :"app_db_user" LOGIN PASSWORD :'app_db_password';
    \echo 'Created application role'
\endif

EOSQL

echo "=========================================="
echo "Application database role is ready."
echo "=========================================="