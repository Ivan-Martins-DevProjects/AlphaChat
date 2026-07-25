-- Migration: Add owner_id to contacts and seed data

-- ============================================================
-- Step 1: Add owner_id column to contacts
-- ============================================================
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS owner_id UUID;
ALTER TABLE contacts ADD CONSTRAINT contacts_owner_id_fkey
    FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE SET NULL;
CREATE INDEX IF NOT EXISTS idx_contacts_owner_id ON contacts(owner_id);

-- ============================================================
-- Step 2: Seed contacts (Fernanda, Juliana, Carlos, Roberto)
-- ============================================================
INSERT INTO contacts (id, name, phone, email, company, document, notes, profile_pic, tags, owner_id, created_at, updated_at) VALUES
    ('a1000000-0000-0000-0000-000000000001', 'Fernanda Silva', '(11) 99876-5432', 'fernanda@startup.io', 'StartupTech', '123.456.789-00', 'Cliente Enterprise', NULL, '[]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NOW()),
    ('a1000000-0000-0000-0000-000000000002', 'Juliana Costa', '(21) 98765-4321', 'juliana@empresa.com.br', 'Costa & Associados', '987.654.321-00', 'Problema com pagamento', NULL, '[]'::jsonb, 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NOW()),
    ('a1000000-0000-0000-0000-000000000003', 'Carlos Oliveira', '(31) 97654-3210', 'carlos@techcorp.com', 'TechCorp', NULL, 'Sugestão de funcionalidade', NULL, '[]'::jsonb, NULL, NOW() - INTERVAL '3 days', NOW()),
    ('a1000000-0000-0000-0000-000000000004', 'Roberto Mendes', '(41) 96543-2109', 'roberto@mendes.com', 'Mendes Ltda', NULL, 'Problema de acesso', NULL, '[]'::jsonb, NULL, NOW() - INTERVAL '2 days', NOW()),
    ('a1000000-0000-0000-0000-000000000005', 'Ana Pereira', '(51) 95432-1098', 'ana@startup.io', 'StartupTech', NULL, 'Interessada no plano Pro', NULL, '[]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '1 day', NOW()),
    ('a1000000-0000-0000-0000-000000000006', 'Pedro Santos', '(61) 94321-0987', 'pedro@enterprise.com', 'Enterprise Corp', NULL, 'Novo cliente potencial', NULL, '[]'::jsonb, NULL, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Step 3: Link existing tickets to contacts
-- ============================================================
UPDATE tickets SET contact_id = 'a1000000-0000-0000-0000-000000000001' WHERE id = '1a000000-0000-0000-0000-000000000001';
UPDATE tickets SET contact_id = 'a1000000-0000-0000-0000-000000000002' WHERE id = '2a000000-0000-0000-0000-000000000002';
UPDATE tickets SET contact_id = 'a1000000-0000-0000-0000-000000000003' WHERE id = '3a000000-0000-0000-0000-000000000003';
UPDATE tickets SET contact_id = 'a1000000-0000-0000-0000-000000000004' WHERE id = '5a000000-0000-0000-0000-000000000005';
