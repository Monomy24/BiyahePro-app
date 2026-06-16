-- ============================================================
-- 07_refresh_tokens.sql
-- Add this table to your database — supports JWT refresh flow
-- ============================================================

CREATE TABLE IF NOT EXISTS user_refresh_tokens (
    id          UUID        PRIMARY KEY DEFAULT uuidv7(),
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  TEXT        NOT NULL UNIQUE,
    expires_at  TIMESTAMPTZ NOT NULL,
    revoked_at  TIMESTAMPTZ,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user
    ON user_refresh_tokens (user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token
    ON user_refresh_tokens (token_hash);

GRANT SELECT, INSERT, UPDATE ON user_refresh_tokens TO ridehailing_app;