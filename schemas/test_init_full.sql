SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;

-- ============== ENUM TYPES ==============
CREATE TYPE public.event_type AS ENUM ('assigned', 'transferred', 'closed', 'reopened');

-- ============== BASE TABLES (from dump, with PKs inline) ==============

CREATE TABLE IF NOT EXISTS public.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL PRIMARY KEY,
    name character varying(100) NOT NULL,
    email character varying(255),
    created_at timestamp without time zone DEFAULT now(),
    role varchar(50),
    password text NOT NULL,
    profile_pic text
);
ALTER TABLE ONLY public.users ADD CONSTRAINT users_email_key UNIQUE (email);

CREATE TABLE IF NOT EXISTS public.tickets (
    id uuid DEFAULT gen_random_uuid() NOT NULL PRIMARY KEY,
    customer_id uuid NOT NULL,
    subject character varying(255),
    status varchar(50) DEFAULT 'open' NOT NULL,
    assigned_to uuid,
    created_at timestamp without time zone DEFAULT now(),
    closed_at timestamp without time zone
);
ALTER TABLE ONLY public.tickets ADD CONSTRAINT tickets_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.users(id);
ALTER TABLE ONLY public.tickets ADD CONSTRAINT tickets_assigned_to_fkey FOREIGN KEY (assigned_to) REFERENCES public.users(id);

CREATE TABLE IF NOT EXISTS public.messages (
    id uuid DEFAULT gen_random_uuid() NOT NULL PRIMARY KEY,
    ticket_id uuid NOT NULL,
    sender_id uuid NOT NULL,
    content text NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    edited_at timestamp without time zone,
    conversation_id UUID,
    message_type VARCHAR(50) NOT NULL DEFAULT 'text',
    is_edited BOOLEAN NOT NULL DEFAULT FALSE,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    updated_at TIMESTAMP
);
ALTER TABLE ONLY public.messages ADD CONSTRAINT messages_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.users(id);
ALTER TABLE ONLY public.messages ADD CONSTRAINT messages_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

CREATE TABLE IF NOT EXISTS public.ticket_events (
    id uuid DEFAULT gen_random_uuid() NOT NULL PRIMARY KEY,
    ticket_id uuid NOT NULL,
    actor_id uuid,
    event_type public.event_type NOT NULL,
    metadata jsonb,
    created_at timestamp without time zone DEFAULT now()
);
ALTER TABLE ONLY public.ticket_events ADD CONSTRAINT ticket_events_actor_id_fkey FOREIGN KEY (actor_id) REFERENCES public.users(id);
ALTER TABLE ONLY public.ticket_events ADD CONSTRAINT ticket_events_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

CREATE TABLE IF NOT EXISTS public.ticket_participants (
    ticket_id uuid NOT NULL,
    user_id uuid NOT NULL,
    role varchar(50) NOT NULL,
    PRIMARY KEY (ticket_id, user_id)
);
ALTER TABLE ONLY public.ticket_participants ADD CONSTRAINT ticket_participants_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.ticket_participants ADD CONSTRAINT ticket_participants_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;

-- ============== ROLES & PERMISSIONS ==============

CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role VARCHAR(50) NOT NULL UNIQUE,
    users_read BOOLEAN NOT NULL DEFAULT FALSE,
    users_create BOOLEAN NOT NULL DEFAULT FALSE,
    users_update BOOLEAN NOT NULL DEFAULT FALSE,
    users_delete BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_read BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_create BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_update BOOLEAN NOT NULL DEFAULT FALSE,
    conversations_delete BOOLEAN NOT NULL DEFAULT FALSE,
    messages_read BOOLEAN NOT NULL DEFAULT FALSE,
    messages_create BOOLEAN NOT NULL DEFAULT FALSE,
    messages_update BOOLEAN NOT NULL DEFAULT FALSE,
    messages_delete BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_read BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_create BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_update BOOLEAN NOT NULL DEFAULT FALSE,
    contacts_delete BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_read BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_create BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_update BOOLEAN NOT NULL DEFAULT FALSE,
    tickets_delete BOOLEAN NOT NULL DEFAULT FALSE,
    tags_read BOOLEAN NOT NULL DEFAULT FALSE,
    tags_create BOOLEAN NOT NULL DEFAULT FALSE,
    tags_update BOOLEAN NOT NULL DEFAULT FALSE,
    tags_delete BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

INSERT INTO roles (name, description) VALUES
    ('admin', 'Administrador do sistema com acesso total'),
    ('agent', 'Atendente com acesso a conversas e tickets'),
    ('viewer', 'Visualizador com acesso somente leitura')
ON CONFLICT (name) DO NOTHING;

INSERT INTO permissions (role,
    users_read, users_create, users_update, users_delete,
    conversations_read, conversations_create, conversations_update, conversations_delete,
    messages_read, messages_create, messages_update, messages_delete,
    contacts_read, contacts_create, contacts_update, contacts_delete,
    tickets_read, tickets_create, tickets_update, tickets_delete,
    tags_read, tags_create, tags_update, tags_delete
) VALUES
    ('admin', TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE),
    ('agent', TRUE, FALSE, FALSE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE),
    ('viewer', TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE)
ON CONFLICT (role) DO NOTHING;

-- ============== NEW TABLES (migration 002) ==============

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
    owner_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);
ALTER TABLE contacts ADD CONSTRAINT contacts_owner_id_fkey
    FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE SET NULL;

CREATE TABLE IF NOT EXISTS tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    color_code VARCHAR(20) NOT NULL DEFAULT '#6366f1',
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS contact_tags (
    contact_id UUID NOT NULL,
    tag_id UUID NOT NULL,
    PRIMARY KEY (contact_id, tag_id),
    CONSTRAINT contact_tags_contact_id_fkey FOREIGN KEY (contact_id)
        REFERENCES contacts(id) ON DELETE CASCADE,
    CONSTRAINT contact_tags_tag_id_fkey FOREIGN KEY (tag_id)
        REFERENCES tags(id) ON DELETE CASCADE
);

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

ALTER TABLE messages ADD CONSTRAINT messages_conversation_id_fkey
    FOREIGN KEY (conversation_id) REFERENCES conversations(id) ON DELETE CASCADE;
ALTER TABLE tickets ADD CONSTRAINT tickets_contact_id_fkey
    FOREIGN KEY (contact_id) REFERENCES contacts(id) ON DELETE SET NULL;

-- ============== SEED DATA ==============

-- Users
INSERT INTO users (id, name, email, role, password, profile_pic, created_at) VALUES
    ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Maria Santos', 'maria@alphachat.com', 'admin', '123456', NULL, NOW() - INTERVAL '30 days'),
    ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'João Silva', 'joao@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '25 days'),
    ('d4e5f6a7-b8c9-0123-defa-234567890123', 'Ana Oliveira', 'ana@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '20 days'),
    ('e5f6a7b8-c9d0-1234-efab-345678901234', 'Pedro Costa', 'pedro@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '15 days'),
    ('f6a7b8c9-d0e1-2345-fabc-456789012345', 'Lucia Ferreira', 'lucia@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '10 days')
ON CONFLICT (id) DO NOTHING;

-- Contacts
INSERT INTO contacts (id, name, phone, email, company, document, notes, profile_pic, tags, owner_id, created_at, updated_at) VALUES
    ('a1000000-0000-0000-0000-000000000001', 'Fernanda Silva', '(11) 99876-5432', 'fernanda@startup.io', 'StartupTech', '123.456.789-00', 'Cliente Enterprise', NULL, '[]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NOW()),
    ('a1000000-0000-0000-0000-000000000002', 'Juliana Costa', '(21) 98765-4321', 'juliana@empresa.com.br', 'Costa & Associados', '987.654.321-00', 'Problema com pagamento', NULL, '[]'::jsonb, 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NOW()),
    ('a1000000-0000-0000-0000-000000000003', 'Carlos Oliveira', '(31) 97654-3210', 'carlos@techcorp.com', 'TechCorp', NULL, 'Sugestão de funcionalidade', NULL, '[]'::jsonb, NULL, NOW() - INTERVAL '3 days', NOW()),
    ('a1000000-0000-0000-0000-000000000004', 'Roberto Mendes', '(41) 96543-2109', 'roberto@mendes.com', 'Mendes Ltda', NULL, 'Problema de acesso', NULL, '[]'::jsonb, NULL, NOW() - INTERVAL '2 days', NOW()),
    ('a1000000-0000-0000-0000-000000000005', 'Ana Pereira', '(51) 95432-1098', 'ana@startup.io', 'StartupTech', NULL, 'Interessada no plano Pro', NULL, '[]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '1 day', NOW()),
    ('a1000000-0000-0000-0000-000000000006', 'Pedro Santos', '(61) 94321-0987', 'pedro@enterprise.com', 'Enterprise Corp', NULL, 'Novo cliente potencial', NULL, '[]'::jsonb, NULL, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Tags
INSERT INTO tags (id, name, color_code) VALUES
    ('f1000000-0000-0000-0000-000000000001', 'Enterprise', '#ef4444'),
    ('f1000000-0000-0000-0000-000000000002', 'Urgente', '#f97316'),
    ('f1000000-0000-0000-0000-000000000003', 'Suporte', '#3b82f6'),
    ('f1000000-0000-0000-0000-000000000004', 'Vendas', '#10b981'),
    ('f1000000-0000-0000-0000-000000000005', 'Novo Cliente', '#8b5cf6')
ON CONFLICT (id) DO NOTHING;

-- Tickets
INSERT INTO tickets (id, customer_id, subject, status, assigned_to, contact_id, created_at, closed_at) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Plano Enterprise - Proposta', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'a1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '7 days', NULL),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Erro na integração de pagamento', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1000000-0000-0000-0000-000000000002', NOW() - INTERVAL '5 days', NULL),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Sugestão: Relatório de vendas', 'open', NULL, 'a1000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 days', NULL),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'Dúvida sobre planos', 'closed', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'a1000000-0000-0000-0000-000000000005', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Problema de acesso - Telegram', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1000000-0000-0000-0000-000000000004', NOW() - INTERVAL '2 days', NULL)
ON CONFLICT (id) DO NOTHING;

-- Ticket participants
INSERT INTO ticket_participants (ticket_id, user_id, role) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'admin'),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'user'),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'user'),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'user'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'user')
ON CONFLICT (ticket_id, user_id) DO NOTHING;

-- Conversations
INSERT INTO conversations (id, type, owner, ticket_id, read, created_at) VALUES
    ('1b000000-0000-0000-0000-000000000001', 'direct', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', '1a000000-0000-0000-0000-000000000001', TRUE, NOW() - INTERVAL '7 days'),
    ('2b000000-0000-0000-0000-000000000002', 'direct', 'c3d4e5f6-a7b8-9012-cdef-123456789012', '2a000000-0000-0000-0000-000000000002', TRUE, NOW() - INTERVAL '5 days'),
    ('3b000000-0000-0000-0000-000000000003', 'direct', 'd4e5f6a7-b8c9-0123-defa-234567890123', '3a000000-0000-0000-0000-000000000003', TRUE, NOW() - INTERVAL '3 days'),
    ('4b000000-0000-0000-0000-000000000004', 'direct', 'e5f6a7b8-c9d0-1234-efab-345678901234', '4a000000-0000-0000-0000-000000000004', TRUE, NOW() - INTERVAL '10 days'),
    ('5b000000-0000-0000-0000-000000000005', 'direct', 'c3d4e5f6-a7b8-9012-cdef-123456789012', '5a000000-0000-0000-0000-000000000005', TRUE, NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO NOTHING;

-- Messages
INSERT INTO messages (id, ticket_id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('1c000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat.', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('1c000000-0000-0000-0000-000000000002', '1a000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
    ('1c000000-0000-0000-0000-000000000003', '1a000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O plano inclui suporte 24/7 e SLA de 1 hora.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 50 minutes'),
    ('2c000000-0000-0000-0000-000000000011', '2a000000-0000-0000-0000-000000000002', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vi que está com problema na integração.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('2c000000-0000-0000-0000-000000000012', '2a000000-0000-0000-0000-000000000002', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei o problema! Timeout na requisição.', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('3c000000-0000-0000-0000-000000000021', '3a000000-0000-0000-0000-000000000003', '3b000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Recebi sua sugestão de funcionalidade.', 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours'),
    ('5c000000-0000-0000-0000-000000000031', '5a000000-0000-0000-0000-000000000005', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vi que está com problema de acesso.', 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes'),
    ('5c000000-0000-0000-0000-000000000032', '5a000000-0000-0000-0000-000000000005', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'IP bloqueado por excesso de tentativas. Desbloqueado.', 'text', FALSE, FALSE, NOW() - INTERVAL '10 minutes')
ON CONFLICT (id) DO NOTHING;

-- Indexes
CREATE INDEX IF NOT EXISTS idx_roles_name ON roles(name);
CREATE INDEX IF NOT EXISTS idx_permissions_role ON permissions(role);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_conversations_ticket_id ON conversations(ticket_id);
CREATE INDEX IF NOT EXISTS idx_conversations_owner ON conversations(owner);
CREATE INDEX IF NOT EXISTS idx_tickets_contact_id ON tickets(contact_id);
CREATE INDEX IF NOT EXISTS idx_contact_tags_contact_id ON contact_tags(contact_id);
CREATE INDEX IF NOT EXISTS idx_contact_tags_tag_id ON contact_tags(tag_id);
CREATE INDEX IF NOT EXISTS idx_contacts_owner_id ON contacts(owner_id);
