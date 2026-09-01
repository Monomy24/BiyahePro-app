-- ============================================================
-- 10_add_booking_fee.sql
-- Migration: adds the ₱5 ancillary booking fee referenced in the
-- BP's break-even analysis (Section VIII.D) as its own tracked field,
-- instead of only being folded silently into fare_amount.
--
-- Only needed if you already ran 03_trips.sql / 06_seed.sql before this
-- fix. Fresh installs get both from those files directly and can skip
-- this file.
-- ============================================================

ALTER TABLE trips ADD COLUMN IF NOT EXISTS booking_fee NUMERIC(10,2) NOT NULL DEFAULT 0;

INSERT INTO app_settings (key, value, data_type, category, label, description, is_public) VALUES
('fare.booking_fee', '5.00', 'number', 'fare', 'Booking fee',
 'Flat ancillary fee added to every completed ride (PHP) — see BP break-even analysis', false)
ON CONFLICT (key) DO NOTHING;