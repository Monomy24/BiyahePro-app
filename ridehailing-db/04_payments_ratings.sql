-- ============================================================
-- 04_payments_ratings.sql
-- ============================================================

CREATE TABLE IF NOT EXISTS payments (
    id              UUID        PRIMARY KEY DEFAULT uuidv7(),
    trip_id         UUID        NOT NULL UNIQUE REFERENCES trips(id),
    amount          NUMERIC(10,2) NOT NULL,
    method          TEXT        NOT NULL CHECK (method IN ('cash', 'gcash', 'card')),
    status          TEXT        NOT NULL DEFAULT 'pending'
                                CHECK (status IN ('pending', 'paid', 'failed', 'refunded')),
    reference_code  TEXT,       -- External payment gateway reference
    paid_at         TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_payments_trip   ON payments (trip_id);
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments (status);

GRANT SELECT, INSERT, UPDATE ON payments TO ridehailing_app;

-- ============================================================
-- Ratings (both customer rates driver AND driver rates customer)
-- ============================================================
CREATE TABLE IF NOT EXISTS ratings (
    id              UUID        PRIMARY KEY DEFAULT uuidv7(),
    trip_id         UUID        NOT NULL REFERENCES trips(id),
    rated_by        UUID        NOT NULL REFERENCES users(id),
    rated_user      UUID        NOT NULL REFERENCES users(id),
    score           SMALLINT    NOT NULL CHECK (score BETWEEN 1 AND 5),
    comment         TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (trip_id, rated_by)  -- One rating per person per trip
);

CREATE INDEX IF NOT EXISTS idx_ratings_trip      ON ratings (trip_id);
CREATE INDEX IF NOT EXISTS idx_ratings_rated_user ON ratings (rated_user);

-- Auto-update driver rating average when a new rating is inserted
CREATE OR REPLACE FUNCTION update_driver_rating()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    v_driver_user_id UUID;
    v_new_avg NUMERIC(3,2);
BEGIN
    -- Find if the rated_user is a driver
    SELECT id INTO v_driver_user_id
    FROM drivers WHERE user_id = NEW.rated_user;

    IF FOUND THEN
        SELECT ROUND(AVG(score)::NUMERIC, 2)
        INTO v_new_avg
        FROM ratings
        WHERE rated_user = NEW.rated_user;

        UPDATE drivers SET rating = v_new_avg
        WHERE user_id = NEW.rated_user;
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trg_update_driver_rating
    AFTER INSERT ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_driver_rating();

GRANT SELECT, INSERT ON ratings TO ridehailing_app;