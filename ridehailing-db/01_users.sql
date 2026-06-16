-- ============================================================
-- 01_users.sql
-- Core user table shared by customers, drivers, and admins
-- ============================================================

CREATE TABLE IF NOT EXISTS users (
    id              UUID        PRIMARY KEY DEFAULT uuidv7(),
    full_name       TEXT        NOT NULL,
    email           TEXT        NOT NULL UNIQUE,
    phone           TEXT        NOT NULL UNIQUE,
    password_hash   TEXT        NOT NULL,
    role            TEXT        NOT NULL DEFAULT 'customer'
                                CHECK (role IN ('customer', 'driver', 'admin')),
    avatar_url      TEXT,
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    is_verified     BOOLEAN     NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_users_email   ON users (email);
CREATE INDEX IF NOT EXISTS idx_users_phone   ON users (phone);
CREATE INDEX IF NOT EXISTS idx_users_role    ON users (role);

-- Trigram index for name search (admin dashboard)
CREATE INDEX IF NOT EXISTS idx_users_name_trgm
    ON users USING GIN (full_name gin_trgm_ops);

-- Auto-update updated_at on every row change
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Grant to app role
GRANT SELECT, INSERT, UPDATE ON users TO ridehailing_app;