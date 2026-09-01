// File path in project: ridehailing-db/12_scheduled_rides.sql
-- ============================================================
-- 12_scheduled_rides.sql
-- Scheduled Ride Booking (BP §III "Future Product/Service Expansion")
--
-- A scheduled trip is created with status = 'scheduled' and a
-- scheduled_for timestamp instead of going straight to 'requested'
-- (which is what triggers driver dispatch). ScheduledTripActivationService
-- (a BackgroundService) flips it to 'requested' once scheduled_for
-- arrives, at which point it enters the normal dispatch flow exactly
-- like an immediate booking.
--
-- Only needed if you already ran 03_trips.sql / 06_seed.sql before this
-- fix. Fresh installs get all of this directly from those files.
-- ============================================================

ALTER TABLE trips ADD COLUMN IF NOT EXISTS scheduled_for TIMESTAMPTZ;

ALTER TABLE trips DROP CONSTRAINT IF EXISTS trips_status_check;
ALTER TABLE trips
    ADD CONSTRAINT trips_status_check
    CHECK (status IN (
        'scheduled', 'requested', 'accepted', 'en_route',
        'arrived', 'in_progress', 'completed', 'cancelled'
    ));

CREATE INDEX IF NOT EXISTS idx_trips_scheduled_pending
    ON trips (scheduled_for)
    WHERE status = 'scheduled';

-- feature.scheduled_rides already exists (06_seed.sql) — this only adds
-- the new minimum-lead-time setting, safe to run even if you've already
-- got feature.scheduled_rides seeded.
INSERT INTO app_settings (key, value, data_type, category, label, description, is_public) VALUES
('ops.scheduled_min_lead_minutes', '30', 'number', 'operations', 'Scheduled ride lead time', 'Earliest a scheduled ride can be booked ahead of its requested time', false)
ON CONFLICT (key) DO NOTHING;