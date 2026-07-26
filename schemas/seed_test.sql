-- Seed data for integration tests

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

-- Tickets (with contact_id)
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

-- Contact tags
INSERT INTO contact_tags (contact_id, tag_id) VALUES
    ('a1000000-0000-0000-0000-000000000001', 'f1000000-0000-0000-0000-000000000001'),
    ('a1000000-0000-0000-0000-000000000001', 'f1000000-0000-0000-0000-000000000002'),
    ('a1000000-0000-0000-0000-000000000002', 'f1000000-0000-0000-0000-000000000003'),
    ('a1000000-0000-0000-0000-000000000005', 'f1000000-0000-0000-0000-000000000004')
ON CONFLICT (contact_id, tag_id) DO NOTHING;
