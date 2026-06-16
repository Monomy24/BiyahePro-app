-- ============================================================
-- 00_extensions.sql
-- Run this FIRST as a superuser (postgres)
-- ============================================================

-- UUID generation (built into PG 18 but explicit for clarity)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- PostGIS: geospatial support (driver locations, pickup/dropoff)
CREATE EXTENSION IF NOT EXISTS "postgis";

-- pg_trgm: fast text search on names, plates, emails
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ============================================================
-- Create the database role for the app (least privilege)
-- ============================================================
DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'ridehailing_app') THEN
    CREATE ROLE ridehailing_app LOGIN PASSWORD 'change_me_in_production';
  END IF;
END
$$;

-- Create the database
-- Run this separately in psql if the DB doesn't exist yet:
-- CREATE DATABASE ridehailing OWNER ridehailing_app;