-- Migration: Add missing tables and columns required by queries
-- Adds: conversations, contacts, tags, contact_tags tables
-- Adds missing columns to messages and tickets

-- ============================================================
-- Step 1: Add missing columns to messages table
-- ============================================================
-- Rename 'message' to 'content' (queries expect 'content')
ALTER TABLE messages RENAME COLUMN message TO content;

-- Add columns that queries need
ALTER TABLE messages ADD COLUMN IF NOT EXISTS conversation_id UUID;
ALTER TABLE messages ADD COLUMN IF NOT EXISTS message_type VARCHAR(50) NOT NULL DEFAULT 'text';
ALTER TABLE messages ADD COLUMN IF NOT EXISTS is_edited BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE messages ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE messages ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;

-- Rename 'edited_at' to match expected pattern (keep for compatibility)
-- ALTER TABLE messages DROP COLUMN IF EXISTS edited_at;

-- ============================================================
-- Step 2: Add missing columns to tickets table
-- ============================================================
ALTER TABLE tickets ADD COLUMN IF NOT EXISTS contact_id UUID;

-- ============================================================
-- Step 3: Create contacts table
-- ============================================================
CREATE TABLE IF NOT EXISTS contacts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255),
    phone VARCHAR(50),
    email VARCHAR(255),
    company VARCHAR(255),
    document VARCHAR(50),
    notes TEXT,
    profile_pic TEXT,
    tags JSONB DEFAULT '[]'::jsonb,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- Step 4: Create conversations table
-- ============================================================
CREATE TABLE IF NOT EXISTS conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL UNIQUE,
    type VARCHAR(50),
    owner UUID,
    read BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT conversations_ticket_id_fkey FOREIGN KEY (ticket_id)
        REFERENCES tickets(id) ON DELETE CASCADE,
    CONSTRAINT conversations_owner_fkey FOREIGN KEY (owner)
        REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================================
-- Step 5: Create tags table
-- ============================================================
CREATE TABLE IF NOT EXISTS tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    color_code VARCHAR(20) NOT NULL DEFAULT '#6366f1',
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- Step 6: Create contact_tags table
-- ============================================================
CREATE TABLE IF NOT EXISTS contact_tags (
    contact_id UUID NOT NULL,
    tag_id UUID NOT NULL,
    PRIMARY KEY (contact_id, tag_id),
    CONSTRAINT contact_tags_contact_id_fkey FOREIGN KEY (contact_id)
        REFERENCES contacts(id) ON DELETE CASCADE,
    CONSTRAINT contact_tags_tag_id_fkey FOREIGN KEY (tag_id)
        REFERENCES tags(id) ON DELETE CASCADE
);

-- ============================================================
-- Step 7: Add foreign key for messages.conversation_id
-- ============================================================
ALTER TABLE messages ADD CONSTRAINT messages_conversation_id_fkey
    FOREIGN KEY (conversation_id) REFERENCES conversations(id) ON DELETE CASCADE;

-- ============================================================
-- Step 8: Add foreign key for tickets.contact_id
-- ============================================================
ALTER TABLE tickets ADD CONSTRAINT tickets_contact_id_fkey
    FOREIGN KEY (contact_id) REFERENCES contacts(id) ON DELETE SET NULL;

-- ============================================================
-- Step 9: Create indexes for better performance
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_conversations_ticket_id ON conversations(ticket_id);
CREATE INDEX IF NOT EXISTS idx_conversations_owner ON conversations(owner);
CREATE INDEX IF NOT EXISTS idx_tickets_contact_id ON tickets(contact_id);
CREATE INDEX IF NOT EXISTS idx_contact_tags_contact_id ON contact_tags(contact_id);
CREATE INDEX IF NOT EXISTS idx_contact_tags_tag_id ON contact_tags(tag_id);
