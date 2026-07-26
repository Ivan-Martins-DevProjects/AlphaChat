--
-- AlphaChat - Schema completo do banco de dados
-- Corresponde ao estado final apos todas as migrations (001-005)
-- Este arquivo contem: EXTENSION, TYPES, TABLES, CONSTRAINTS, INDEXES
-- Nao contem dados (seed) - usar seed_qas.sql para isso
--

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

-- ============================================================
-- EXTENSIONS
-- ============================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;
COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';

-- ============================================================
-- TYPES
-- ============================================================

CREATE TYPE public.event_type AS ENUM (
    'assigned',
    'transferred',
    'closed',
    'reopened'
);

-- ============================================================
-- TABLES
-- ============================================================

-- Tabela: users
CREATE TABLE public.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    email character varying(255),
    created_at timestamp without time zone DEFAULT now(),
    role varchar(50),
    password text NOT NULL,
    profile_pic text
);

-- Tabela: contacts
CREATE TABLE public.contacts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(255),
    phone character varying(50),
    email character varying(255),
    company character varying(255),
    document character varying(50),
    notes text,
    profile_pic text,
    tags jsonb DEFAULT '[]'::jsonb,
    owner_id uuid,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now()
);

-- Tabela: tickets
CREATE TABLE public.tickets (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    customer_id uuid NOT NULL,
    subject character varying(255),
    status varchar(50) DEFAULT 'open' NOT NULL,
    assigned_to uuid,
    created_at timestamp without time zone DEFAULT now(),
    closed_at timestamp without time zone,
    contact_id uuid
);

-- Tabela: conversations
CREATE TABLE public.conversations (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    ticket_id uuid NOT NULL,
    type character varying(50),
    owner uuid,
    read boolean NOT NULL DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now()
);

-- Tabela: messages
CREATE TABLE public.messages (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    ticket_id uuid NOT NULL,
    sender_id uuid NOT NULL,
    content text NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    edited_at timestamp without time zone,
    conversation_id uuid,
    message_type character varying(50) NOT NULL DEFAULT 'text',
    is_edited boolean NOT NULL DEFAULT false,
    is_deleted boolean NOT NULL DEFAULT false,
    updated_at timestamp without time zone
);

-- Tabela: ticket_events
CREATE TABLE public.ticket_events (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    ticket_id uuid NOT NULL,
    actor_id uuid,
    event_type public.event_type NOT NULL,
    metadata jsonb,
    created_at timestamp without time zone DEFAULT now()
);

-- Tabela: ticket_participants
CREATE TABLE public.ticket_participants (
    ticket_id uuid NOT NULL,
    user_id uuid NOT NULL,
    role character varying(50) NOT NULL
);

-- Tabela: roles
CREATE TABLE public.roles (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now()
);

-- Tabela: permissions
CREATE TABLE public.permissions (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    role character varying(50) NOT NULL,

    users_read boolean NOT NULL DEFAULT false,
    users_create boolean NOT NULL DEFAULT false,
    users_update boolean NOT NULL DEFAULT false,
    users_delete boolean NOT NULL DEFAULT false,

    conversations_read boolean NOT NULL DEFAULT false,
    conversations_create boolean NOT NULL DEFAULT false,
    conversations_update boolean NOT NULL DEFAULT false,
    conversations_delete boolean NOT NULL DEFAULT false,

    messages_read boolean NOT NULL DEFAULT false,
    messages_create boolean NOT NULL DEFAULT false,
    messages_update boolean NOT NULL DEFAULT false,
    messages_delete boolean NOT NULL DEFAULT false,

    contacts_read boolean NOT NULL DEFAULT false,
    contacts_create boolean NOT NULL DEFAULT false,
    contacts_update boolean NOT NULL DEFAULT false,
    contacts_delete boolean NOT NULL DEFAULT false,

    tickets_read boolean NOT NULL DEFAULT false,
    tickets_create boolean NOT NULL DEFAULT false,
    tickets_update boolean NOT NULL DEFAULT false,
    tickets_delete boolean NOT NULL DEFAULT false,

    tags_read boolean NOT NULL DEFAULT false,
    tags_create boolean NOT NULL DEFAULT false,
    tags_update boolean NOT NULL DEFAULT false,
    tags_delete boolean NOT NULL DEFAULT false,

    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now()
);

-- Tabela: tags
CREATE TABLE public.tags (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    color_code character varying(20) NOT NULL DEFAULT '#6366f1',
    created_at timestamp without time zone DEFAULT now()
);

-- Tabela: contact_tags
CREATE TABLE public.contact_tags (
    contact_id uuid NOT NULL,
    tag_id uuid NOT NULL
);

-- ============================================================
-- PRIMARY KEYS
-- ============================================================

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.contacts
    ADD CONSTRAINT contacts_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.conversations
    ADD CONSTRAINT conversations_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.ticket_events
    ADD CONSTRAINT ticket_events_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.ticket_participants
    ADD CONSTRAINT ticket_participants_pkey PRIMARY KEY (ticket_id, user_id);

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT permissions_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.contact_tags
    ADD CONSTRAINT contact_tags_pkey PRIMARY KEY (contact_id, tag_id);

-- ============================================================
-- UNIQUE CONSTRAINTS
-- ============================================================

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);

ALTER TABLE ONLY public.conversations
    ADD CONSTRAINT conversations_ticket_id_unique UNIQUE (ticket_id);

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_name_unique UNIQUE (name);

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT permissions_role_unique UNIQUE (role);

-- ============================================================
-- FOREIGN KEYS
-- ============================================================

-- tickets -> users (customer_id)
ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.users(id);

-- tickets -> users (assigned_to)
ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_assigned_to_fkey FOREIGN KEY (assigned_to) REFERENCES public.users(id);

-- tickets -> contacts
ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_contact_id_fkey FOREIGN KEY (contact_id) REFERENCES public.contacts(id) ON DELETE SET NULL;

-- contacts -> users (owner_id)
ALTER TABLE ONLY public.contacts
    ADD CONSTRAINT contacts_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.users(id) ON DELETE SET NULL;

-- conversations -> tickets
ALTER TABLE ONLY public.conversations
    ADD CONSTRAINT conversations_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

-- conversations -> users (owner)
ALTER TABLE ONLY public.conversations
    ADD CONSTRAINT conversations_owner_fkey FOREIGN KEY (owner) REFERENCES public.users(id) ON DELETE SET NULL;

-- messages -> tickets
ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

-- messages -> users (sender_id)
ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.users(id);

-- messages -> conversations
ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_conversation_id_fkey FOREIGN KEY (conversation_id) REFERENCES public.conversations(id) ON DELETE CASCADE;

-- ticket_events -> tickets
ALTER TABLE ONLY public.ticket_events
    ADD CONSTRAINT ticket_events_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

-- ticket_events -> users (actor_id)
ALTER TABLE ONLY public.ticket_events
    ADD CONSTRAINT ticket_events_actor_id_fkey FOREIGN KEY (actor_id) REFERENCES public.users(id);

-- ticket_participants -> tickets
ALTER TABLE ONLY public.ticket_participants
    ADD CONSTRAINT ticket_participants_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;

-- ticket_participants -> users
ALTER TABLE ONLY public.ticket_participants
    ADD CONSTRAINT ticket_participants_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;

-- contact_tags -> contacts
ALTER TABLE ONLY public.contact_tags
    ADD CONSTRAINT contact_tags_contact_id_fkey FOREIGN KEY (contact_id) REFERENCES public.contacts(id) ON DELETE CASCADE;

-- contact_tags -> tags
ALTER TABLE ONLY public.contact_tags
    ADD CONSTRAINT contact_tags_tag_id_fkey FOREIGN KEY (tag_id) REFERENCES public.tags(id) ON DELETE CASCADE;

-- ============================================================
-- INDEXES
-- ============================================================

CREATE INDEX IF NOT EXISTS idx_users_role ON public.users(role);
CREATE INDEX IF NOT EXISTS idx_contacts_owner_id ON public.contacts(owner_id);
CREATE INDEX IF NOT EXISTS idx_tickets_contact_id ON public.tickets(contact_id);
CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON public.messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_conversations_ticket_id ON public.conversations(ticket_id);
CREATE INDEX IF NOT EXISTS idx_conversations_owner ON public.conversations(owner);
CREATE INDEX IF NOT EXISTS idx_contact_tags_contact_id ON public.contact_tags(contact_id);
CREATE INDEX IF NOT EXISTS idx_contact_tags_tag_id ON public.contact_tags(tag_id);
CREATE INDEX IF NOT EXISTS idx_roles_name ON public.roles(name);
CREATE INDEX IF NOT EXISTS idx_permissions_role ON public.permissions(role);
