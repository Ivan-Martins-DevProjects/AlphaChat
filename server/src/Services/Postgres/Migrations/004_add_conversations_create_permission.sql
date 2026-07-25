-- Migration: Add conversations_create permission column

-- ============================================================
-- Step 1: Add conversations_create column
-- ============================================================
ALTER TABLE permissions ADD COLUMN IF NOT EXISTS conversations_create BOOLEAN NOT NULL DEFAULT FALSE;

-- ============================================================
-- Step 2: Update admin permissions (full access)
-- ============================================================
UPDATE permissions SET conversations_create = TRUE WHERE role = 'admin';

-- ============================================================
-- Step 3: Update agent permissions (can create conversations)
-- ============================================================
UPDATE permissions SET conversations_create = TRUE WHERE role = 'agent';
