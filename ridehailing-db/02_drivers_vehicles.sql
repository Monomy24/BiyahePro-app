-- ============================================================
-- 02_drivers_vehicles.sql
-- Driver profiles, vehicles, and live location tracking
-- ============================================================

-- Driver profile (extends users where role = 'driver')
CREATE TABLE IF NOT EXISTS drivers (
    id                  UUID        PRIMARY KEY DEFAULT uuidv7(),
    user_id             UUID        NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    license_number      TEXT        NOT NULL UNIQUE,
    license_expiry      DATE        NOT NULL,
    status              TEXT        NOT NULL DEFAULT 'offline'
                                    CHECK (status IN ('offline', 'available', 'on_trip', 'suspended')),
    current_location    GEOGRAPHY(POINT, 4326),   -- PostGIS: lat/lng live position
    rating              NUMERIC(3,2) NOT NULL DEFAULT 5.00,
    total_trips         INT         NOT NULL DEFAULT 0,
    is_documents_verified BOOLEAN   NOT NULL DEFAULT false,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Spatial index for "find drivers near me" queries (ST_DWithin)
CREATE INDEX IF NOT EXISTS idx_drivers_location
    ON drivers USING GIST (current_location);

CREATE INDEX IF NOT EXISTS idx_drivers_status
    ON drivers (status);

CREATE OR REPLACE TRIGGER trg_drivers_updated_at
    BEFORE UPDATE ON drivers
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

GRANT SELECT, INSERT, UPDATE ON drivers TO ridehailing_app;

-- ============================================================
-- Vehicles
-- ============================================================
CREATE TABLE IF NOT EXISTS vehicles (
    id              UUID        PRIMARY KEY DEFAULT uuidv7(),
    driver_id       UUID        NOT NULL UNIQUE REFERENCES drivers(id) ON DELETE CASCADE,
    plate_number    TEXT        NOT NULL UNIQUE,
    make            TEXT        NOT NULL,   -- e.g. Toyota
    model           TEXT        NOT NULL,   -- e.g. Vios
    color           TEXT        NOT NULL,
    year            SMALLINT    NOT NULL,
    vehicle_type    TEXT        NOT NULL DEFAULT 'sedan'
                                CHECK (vehicle_type IN ('sedan', 'suv', 'van', 'motorcycle')),
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_vehicles_plate
    ON vehicles USING GIN (plate_number gin_trgm_ops);

GRANT SELECT, INSERT, UPDATE ON vehicles TO ridehailing_app;