-- File path in project: ridehailing-db/11_driver_dpi_strikes.sql
-- ============================================================
-- 11_driver_dpi_strikes.sql
-- Migration: Driver Performance Index + Three-Strike Policy
-- (BP §VI Quality Control, §IX Risk Analysis)
--
-- Only needed if you already ran 02_drivers_vehicles.sql /
-- 04_payments_ratings.sql before this fix. Fresh installs get all of
-- this directly from those files and can skip this file.
-- ============================================================

ALTER TABLE drivers ADD COLUMN IF NOT EXISTS dpi_review_flag BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE drivers ADD COLUMN IF NOT EXISTS strike_count SMALLINT NOT NULL DEFAULT 0;
ALTER TABLE drivers ADD COLUMN IF NOT EXISTS suspended_until TIMESTAMPTZ;

ALTER TABLE drivers DROP CONSTRAINT IF EXISTS drivers_status_check;
ALTER TABLE drivers
    ADD CONSTRAINT drivers_status_check
    CHECK (status IN ('offline', 'available', 'on_trip', 'suspended', 'banned'));

CREATE TABLE IF NOT EXISTS driver_strikes (
    id              UUID        PRIMARY KEY DEFAULT uuidv7(),
    driver_id       UUID        NOT NULL REFERENCES drivers(id) ON DELETE CASCADE,
    strike_number   SMALLINT    NOT NULL CHECK (strike_number BETWEEN 1 AND 3),
    reason          TEXT        NOT NULL,
    consequence     TEXT        NOT NULL,
    issued_by       UUID        NOT NULL REFERENCES users(id),
    issued_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_driver_strikes_driver ON driver_strikes (driver_id, issued_at DESC);

GRANT SELECT, INSERT ON driver_strikes TO ridehailing_app;

-- Replace the rating trigger so it also maintains dpi_review_flag
-- (rating < 4.2 → flagged for review, per BP §VI).
CREATE OR REPLACE FUNCTION update_driver_rating()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    v_driver_user_id UUID;
    v_new_avg NUMERIC(3,2);
BEGIN
    SELECT id INTO v_driver_user_id
    FROM drivers WHERE user_id = NEW.rated_user;

    IF FOUND THEN
        SELECT ROUND(AVG(score)::NUMERIC, 2)
        INTO v_new_avg
        FROM ratings
        WHERE rated_user = NEW.rated_user;

        UPDATE drivers
        SET rating = v_new_avg,
            dpi_review_flag = (v_new_avg < 4.2)
        WHERE user_id = NEW.rated_user;
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trg_update_driver_rating
    AFTER INSERT ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_driver_rating();