#!/bin/bash

set -e

echo "=========================================="
echo "Creating BiyahePro application database role"
echo "=========================================="

psql \
  -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  -v app_db_user="$APP_DB_USER" \
  -v app_db_password="$APP_DB_PASSWORD" <<'EOSQL'

DO $do$
BEGIN

    IF NOT EXISTS (
        SELECT FROM pg_roles
        WHERE rolname = :'app_db_user'
    ) THEN

        EXECUTE format(
            'CREATE ROLE %I LOGIN PASSWORD %L',
            :'app_db_user',
            :'app_db_password'
        );

        RAISE NOTICE 'Created application role: %', :'app_db_user';

    ELSE

        EXECUTE format(
            'ALTER ROLE %I WITH PASSWORD %L',
            :'app_db_user',
            :'app_db_password'
        );

        RAISE NOTICE 'Updated password for application role: %', :'app_db_user';

    END IF;

END
$do$;

EOSQL

echo "=========================================="
echo "Application database role is ready."
echo "=========================================="