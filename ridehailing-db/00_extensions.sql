-- ============================================================
-- 00_extensions.sql
-- Run this FIRST as a superuser (postgres)
-- ============================================================

-- ============================================================
-- PostgreSQL Extensions
-- ============================================================

-- UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- PostGIS: geospatial support
-- Used for driver locations, pickup/dropoff locations, etc.
CREATE EXTENSION IF NOT EXISTS "postgis";

-- pg_trgm: fast text search
-- Used for names, plates, emails, etc.
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ============================================================
-- Database Role
-- ============================================================
--
-- The ridehailing_app role is created separately by Docker
-- using APP_DB_USER and APP_DB_PASSWORD from the .env file.
--
-- Do NOT put the database password directly in this SQL file.
--
-- ============================================================

-- ============================================================
-- Database Creation
-- ============================================================
--
-- The database is created by Docker Compose using:
--
-- POSTGRES_DB
-- POSTGRES_USER
-- POSTGRES_PASSWORD
--
-- Therefore, database creation does not need to happen here.
--
-- ============================================================