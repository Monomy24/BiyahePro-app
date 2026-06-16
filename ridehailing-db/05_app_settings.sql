-- ============================================================
-- 05_app_settings.sql
-- Dynamic settings table — the heart of the "no redeployment"
-- config system. Backend reads + caches these at startup.
-- ============================================================

CREATE TABLE IF NOT EXISTS app_settings (
    id          UUID        PRIMARY KEY DEFAULT uuidv7(),
    key         TEXT        NOT NULL UNIQUE,
    value       TEXT        NOT NULL,
    data_type   TEXT        NOT NULL DEFAULT 'string'
                            CHECK (data_type IN ('string', 'number', 'boolean', 'json')),
    category    TEXT        NOT NULL DEFAULT 'general'
                            CHECK (category IN (
                                'fare',
                                'surge',
                                'operations',
                                'features',
                                'notifications',
                                'general'
                            )),
    label       TEXT        NOT NULL,   -- Human-readable label for admin UI
    description TEXT,
    is_public   BOOLEAN     NOT NULL DEFAULT false, -- expose to mobile app?
    updated_by  UUID        REFERENCES users(id),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_app_settings_category ON app_settings (category);
CREATE INDEX IF NOT EXISTS idx_app_settings_key      ON app_settings (key);

CREATE OR REPLACE TRIGGER trg_app_settings_updated_at
    BEFORE UPDATE ON app_settings
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Admin audit log — every change to app_settings is recorded
CREATE TABLE IF NOT EXISTS admin_audit_log (
    id          UUID        PRIMARY KEY DEFAULT uuidv7(),
    admin_id    UUID        NOT NULL REFERENCES users(id),
    action      TEXT        NOT NULL,   -- e.g. 'UPDATE_SETTING', 'SUSPEND_DRIVER'
    entity_type TEXT        NOT NULL,   -- e.g. 'app_settings', 'drivers'
    entity_id   TEXT,
    old_value   JSONB,
    new_value   JSONB,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_audit_admin    ON admin_audit_log (admin_id);
CREATE INDEX IF NOT EXISTS idx_audit_entity   ON admin_audit_log (entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_created  ON admin_audit_log (created_at DESC);

GRANT SELECT, UPDATE ON app_settings TO ridehailing_app;
GRANT SELECT, INSERT ON admin_audit_log TO ridehailing_app;