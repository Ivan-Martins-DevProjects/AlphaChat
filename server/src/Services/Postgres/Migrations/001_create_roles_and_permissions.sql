-- Migration: Create roles and permissions tables
-- Removes the user_role enum and creates dynamic roles
-- Safe to run: uses IF EXISTS / IF NOT EXISTS throughout

-- ============================================================
-- Step 1: Convert columns from enum to VARCHAR BEFORE drop
-- ============================================================
-- The USING clause converts existing enum values to text
ALTER TABLE ticket_participants
    ALTER COLUMN role TYPE VARCHAR(50) USING role::TEXT;

ALTER TABLE users
    ALTER COLUMN role TYPE VARCHAR(50) USING role::TEXT;

-- ============================================================
-- Step 2: Drop the user_role enum type (now safe)
-- ============================================================
DROP TYPE IF EXISTS user_role;

-- ============================================================
-- Step 3: Create roles table
-- ============================================================
CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- Step 4: Create permissions table
-- ============================================================
CREATE TABLE IF NOT EXISTS permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role VARCHAR(50) NOT NULL UNIQUE,

    -- Users
    users_read BOOLEAN NOT NULL DEFAULT FALSE,
    users_create BOOLEAN NOT NULL DEFAULT FALSE,
    users_update BOOLEAN NOT NULL DEFAULT FALSE,
    users_delete BOOLEAN NOT NULL DEFAULT FALSE,

    -- Conversations
    conversations_read BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_create BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_update BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_delete BOOLEAN NOT NULL DEFAULT FALSE,

    -- Messages
    messages_read BOOLEAN NOT NULL DEFAULT FALSE,
    messages_create BOOLEAN NOT NULL DEFAULT FALSE,
    messages_update BOOLEAN NOT NULL DEFAULT FALSE,
    messages_delete BOOLEAN NOT NULL DEFAULT FALSE,

    -- Contacts
    contacts_read BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_create BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_update BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_delete BOOLEAN NOT NULL DEFAULT FALSE,

    -- Tickets
    tickets_read BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_create BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_update BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_delete BOOLEAN NOT NULL DEFAULT FALSE,

    -- Tags
    tags_read BOOLEAN NOT NULL DEFAULT FALSE,
    tags_create BOOLEAN NOT NULL DEFAULT FALSE,
    tags_update BOOLEAN NOT NULL DEFAULT FALSE,
    tags_delete BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- Step 5: Insert default roles
-- ============================================================
INSERT INTO roles (name, description) VALUES
    ('admin', 'Administrador do sistema com acesso total'),
    ('agent', 'Atendente com acesso a conversas e tickets'),
    ('viewer', 'Visualizador com acesso somente leitura')
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- Step 6: Insert default permissions
-- ============================================================
INSERT INTO permissions (role,
    users_read, users_create, users_update, users_delete,
    conversations_read, conversations_create, conversations_update, conversations_delete,
    messages_read, messages_create, messages_update, messages_delete,
    contacts_read, contacts_create, contacts_update, contacts_delete,
    tickets_read, tickets_create, tickets_update, tickets_delete,
    tags_read, tags_create, tags_update, tags_delete
) VALUES
    -- admin: full access
    ('admin',
     TRUE, TRUE, TRUE, TRUE,
     TRUE, TRUE, TRUE, TRUE,
     TRUE, TRUE, TRUE, TRUE,
     TRUE, TRUE, TRUE, TRUE,
     TRUE, TRUE, TRUE, TRUE,
     TRUE, TRUE, TRUE, TRUE),
    -- agent: read all, write conversations/messages/contacts/tickets
    ('agent',
     TRUE, FALSE, FALSE, FALSE,
     TRUE, TRUE, TRUE, FALSE,
     TRUE, TRUE, TRUE, FALSE,
     TRUE, FALSE, TRUE, FALSE,
     TRUE, FALSE, TRUE, FALSE,
     TRUE, FALSE, FALSE, FALSE),
    -- viewer: read only
    ('viewer',
     TRUE, FALSE, FALSE, FALSE,
     TRUE, FALSE, FALSE, FALSE,
     TRUE, FALSE, FALSE, FALSE,
     TRUE, FALSE, FALSE, FALSE,
     TRUE, FALSE, FALSE, FALSE,
     TRUE, FALSE, FALSE, FALSE)
ON CONFLICT (role) DO NOTHING;

-- ============================================================
-- Step 7: Create indexes
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_roles_name ON roles(name);
CREATE INDEX IF NOT EXISTS idx_permissions_role ON permissions(role);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
