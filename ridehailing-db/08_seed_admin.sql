-- ============================================================
-- 08_seed_admin.sql
-- Creates a real, login-able admin account for local development.
--
-- SettingsController now requires [Authorize(Roles = "admin")] and reads
-- the admin id from the JWT instead of a hardcoded mock GUID, so you need
-- an actual admin user in the `users` table to log in through
-- POST /api/auth/login and get a token that can hit /api/settings.
--
-- Password hash is generated with pgcrypto's crypt()/gen_salt('bf', ...),
-- which produces standard bcrypt hashes ($2a$/$2b$) — the same format
-- BCrypt.Net-Next (used by AuthService.cs) verifies against. No app code
-- needs to run for this to work.
--
-- Run this AFTER 01_users.sql (and after 00_extensions.sql, since it
-- needs pgcrypto for crypt()/gen_salt()).
-- ============================================================

-- ⚠️ DEV-ONLY CREDENTIALS — change the password below before using this
-- anywhere near production, and never commit real prod credentials here.
--   email:    admin@biyahepro.local
--   password: ChangeMe123!

INSERT INTO users (full_name, email, phone, password_hash, role, is_active, is_verified)
VALUES (
    'BiyahePro Admin',
    'admin@biyahepro.local',
    '+639000000000',
    crypt('ChangeMe123!', gen_salt('bf', 11)),
    'admin',
    true,
    true
)
ON CONFLICT (email) DO UPDATE
    SET password_hash = EXCLUDED.password_hash,
        role           = 'admin',
        is_active      = true,
        is_verified    = true;

-- To reset the password later, just re-run this file with a new literal
-- in place of 'ChangeMe123!' — the ON CONFLICT clause overwrites the hash.