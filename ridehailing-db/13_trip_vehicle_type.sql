-- File path in project: ridehailing-db/13_trip_vehicle_type.sql
-- ============================================================
-- 13_trip_vehicle_type.sql
-- Adds requested vehicle type to trips (BP §III "Vehicle Options" —
-- motorcycle vs. motorcab/baobao). Lets a passenger choose ride type,
-- and lets TripService.AcceptAsync reject a driver whose registered
-- vehicle doesn't match what was requested.
--
-- Only needed if you already ran 03_trips.sql before this fix. Fresh
-- installs get this directly from that file.
-- ============================================================

ALTER TABLE trips ADD COLUMN IF NOT EXISTS vehicle_type TEXT NOT NULL DEFAULT 'motorcycle';

ALTER TABLE trips DROP CONSTRAINT IF EXISTS trips_vehicle_type_check;
ALTER TABLE trips
    ADD CONSTRAINT trips_vehicle_type_check
    CHECK (vehicle_type IN ('motorcycle', 'motorcab'));