-- ============================================================
-- AlphaChat - Seed Data para ambiente QAS
-- Tabelas: users, contacts, tickets, conversations, messages,
--          ticket_events, ticket_participants, roles, permissions,
--          tags, contact_tags
-- ============================================================

-- ============================================================
-- Usuários (tela de login e listagem de atendentes)
-- ============================================================
INSERT INTO users (id, name, email, role, password, profile_pic, created_at) VALUES
    ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Maria Santos', 'maria@alphachat.com', 'admin', '123456', NULL, NOW() - INTERVAL '30 days'),
    ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'João Silva', 'joao@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '25 days'),
    ('d4e5f6a7-b8c9-0123-defa-234567890123', 'Ana Oliveira', 'ana@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '20 days'),
    ('e5f6a7b8-c9d0-1234-efab-345678901234', 'Pedro Costa', 'pedro@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '15 days'),
    ('f6a7b8c9-d0e1-2345-fabc-456789012345', 'Lucia Ferreira', 'lucia@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '10 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Roles
-- ============================================================
INSERT INTO roles (id, name, description, created_at) VALUES
    ('a1000000-0000-0000-0000-000000000001', 'admin', 'Administrador com acesso total ao sistema', NOW() - INTERVAL '60 days'),
    ('a1000000-0000-0000-0000-000000000002', 'agent', 'Atendente com acesso a tickets e mensagens', NOW() - INTERVAL '60 days'),
    ('a1000000-0000-0000-0000-000000000003', 'viewer', 'Somente visualização', NOW() - INTERVAL '60 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Permissions
-- ============================================================
INSERT INTO permissions (id, role, users_read, users_create, users_update, users_delete,
    conversations_read, conversations_create, conversations_update, conversations_delete,
    messages_read, messages_create, messages_update, messages_delete,
    contacts_read, contacts_create, contacts_update, contacts_delete,
    tickets_read, tickets_create, tickets_update, tickets_delete,
    tags_read, tags_create, tags_update, tags_delete) VALUES
    ('b1000000-0000-0000-0000-000000000001', 'admin', TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE),
    ('b1000000-0000-0000-0000-000000000002', 'agent', TRUE, FALSE, FALSE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE),
    ('b1000000-0000-0000-0000-000000000003', 'viewer', TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE)
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Tags
-- ============================================================
INSERT INTO tags (id, name, color_code, created_at) VALUES
    ('c1000000-0000-0000-0000-000000000001', 'urgente', '#ef4444', NOW() - INTERVAL '30 days'),
    ('c1000000-0000-0000-0000-000000000002', 'financeiro', '#f59e0b', NOW() - INTERVAL '30 days'),
    ('c1000000-0000-0000-0000-000000000003', 'suporte', '#3b82f6', NOW() - INTERVAL '30 days'),
    ('c1000000-0000-0000-0000-000000000004', 'feature-request', '#8b5cf6', NOW() - INTERVAL '30 days'),
    ('c1000000-0000-0000-0000-000000000005', 'vip', '#10b981', NOW() - INTERVAL '30 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Contatos
-- ============================================================
INSERT INTO contacts (id, name, phone, email, company, document, notes, profile_pic, tags, owner_id, created_at) VALUES
    ('d1000000-0000-0000-0000-000000000001', 'TechCorp Brasil', '+5511999887766', 'contato@techcorp.com.br', 'TechCorp Ltda', '12.345.678/0001-90', 'Empresa de tecnologia - cliente enterprise', NULL, '["vip", "enterprise"]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '25 days'),
    ('d1000000-0000-0000-0000-000000000002', 'StartupXYZ', '+5511988776655', 'suporte@startupxyz.com', 'StartupXYZ Inc', '98.765.432/0001-10', 'Startup em crescimento - plano pro', NULL, '["pro"]'::jsonb, 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '20 days'),
    ('d1000000-0000-0000-0000-000000000003', 'Comércio Express', '+5521977665544', 'vendas@comercioexpress.com', 'Comércio Express ME', '45.678.901/0001-23', 'Loja online - integração pagamento', NULL, '["urgente", "financeiro"]'::jsonb, 'd4e5f6a7-b8c9-0123-defa-234567890123', NOW() - INTERVAL '15 days'),
    ('d1000000-0000-0000-0000-000000000004', 'Digital Agency', '+5531966554433', 'contato@digitalagency.com.br', 'Digital Agency SARL', '67.890.123/0001-45', 'Agência de marketing digital', NULL, '["pro"]'::jsonb, 'e5f6a7b8-c9d0-1234-efab-345678901234', NOW() - INTERVAL '10 days'),
    ('d1000000-0000-0000-0000-000000000005', 'Indústria ABC', '+5541955443322', 'ti@industriaabc.com', 'Indústria ABC S.A.', '23.456.789/0001-67', 'Indústria de transformação', NULL, '["enterprise"]'::jsonb, 'f6a7b8c9-d0e1-2345-fabc-456789012345', NOW() - INTERVAL '5 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Tickets
-- ============================================================
INSERT INTO tickets (id, customer_id, subject, status, assigned_to, created_at, closed_at, contact_id) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Plano Enterprise - Proposta', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NULL, 'd1000000-0000-0000-0000-000000000001'),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Erro na integração de pagamento', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NULL, 'd1000000-0000-0000-0000-000000000002'),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Sugestão: Relatório de vendas', 'open', NULL, NOW() - INTERVAL '3 days', NULL, 'd1000000-0000-0000-0000-000000000003'),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'Dúvida sobre planos', 'closed', 'e5f6a7b8-c9d0-1234-efab-345678901234', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days', 'd1000000-0000-0000-0000-000000000004'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Problema de acesso - Telegram', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '2 days', NULL, 'd1000000-0000-0000-0000-000000000005')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Conversations
-- ============================================================
INSERT INTO conversations (id, ticket_id, type, owner, read, created_at) VALUES
    ('e1000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', 'whatsapp', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', TRUE, NOW() - INTERVAL '7 days'),
    ('e1000000-0000-0000-0000-000000000002', '2a000000-0000-0000-0000-000000000002', 'telegram', 'c3d4e5f6-a7b8-9012-cdef-123456789012', TRUE, NOW() - INTERVAL '5 days'),
    ('e1000000-0000-0000-0000-000000000003', '3a000000-0000-0000-0000-000000000003', 'web', NULL, TRUE, NOW() - INTERVAL '3 days'),
    ('e1000000-0000-0000-0000-000000000004', '4a000000-0000-0000-0000-000000000004', 'whatsapp', 'e5f6a7b8-c9d0-1234-efab-345678901234', TRUE, NOW() - INTERVAL '10 days'),
    ('e1000000-0000-0000-0000-000000000005', '5a000000-0000-0000-0000-000000000005', 'telegram', 'c3d4e5f6-a7b8-9012-cdef-123456789012', TRUE, NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Participantes dos tickets
-- ============================================================
INSERT INTO ticket_participants (ticket_id, user_id, role) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'admin'),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'user'),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'user'),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'user'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'user')
ON CONFLICT (ticket_id, user_id) DO NOTHING;

-- ============================================================
-- Ticket Events
-- ============================================================
INSERT INTO ticket_events (id, ticket_id, actor_id, event_type, metadata, created_at) VALUES
    ('f1000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'assigned', '{"assigned_to": "Maria Santos"}'::jsonb, NOW() - INTERVAL '7 days'),
    ('f1000000-0000-0000-0000-000000000002', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'assigned', '{"assigned_to": "João Silva"}'::jsonb, NOW() - INTERVAL '5 days'),
    ('f1000000-0000-0000-0000-000000000003', '4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'closed', '{"reason": "Cliente satisfeito"}'::jsonb, NOW() - INTERVAL '8 days'),
    ('f1000000-0000-0000-0000-000000000004', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'assigned', '{"assigned_to": "João Silva"}'::jsonb, NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Mensagens
-- ============================================================

-- Ticket 1: Maria - Plano Enterprise
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('1c000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat. Como posso ajudar?', 'e1000000-0000-0000-0000-000000000001', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('1c000000-0000-0000-0000-000000000002', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise. Posso te explicar as vantagens?', 'e1000000-0000-0000-0000-000000000001', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
    ('1c000000-0000-0000-0000-000000000003', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O plano Enterprise inclui: suporte 24/7, SLA de 1 hora, integrações ilimitadas.', 'e1000000-0000-0000-0000-000000000001', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 50 minutes'),
    ('1c000000-0000-0000-0000-000000000004', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O valor é R$ 499,90/mês. Posso enviar a proposta?', 'e1000000-0000-0000-0000-000000000001', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 40 minutes'),
    ('1c000000-0000-0000-0000-000000000005', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Perfeito! Vou enviar a proposta agora.', 'e1000000-0000-0000-0000-000000000001', 'text', FALSE, FALSE, NOW() - INTERVAL '5 minutes')
ON CONFLICT (id) DO NOTHING;

-- Ticket 2: João - Erro Pagamento
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('2c000000-0000-0000-0000-000000000011', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá! Vi que está com problema na integração de pagamento.', 'e1000000-0000-0000-0000-000000000002', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('2c000000-0000-0000-0000-000000000012', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Pode me enviar o ID da transação que está falhando?', 'e1000000-0000-0000-0000-000000000002', 'text', FALSE, FALSE, NOW() - INTERVAL '23 hours'),
    ('2c000000-0000-0000-0000-000000000013', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei o problema! Timeout na requisição. Vou ajustar para 30s.', 'e1000000-0000-0000-0000-000000000002', 'text', FALSE, FALSE, NOW() - INTERVAL '20 hours'),
    ('2c000000-0000-0000-0000-000000000014', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Ajuste realizado. Pode testar novamente?', 'e1000000-0000-0000-0000-000000000002', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours')
ON CONFLICT (id) DO NOTHING;

-- Ticket 3: Ana - Sugestão
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('3c000000-0000-0000-0000-000000000021', '3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Olá! Recebi sua sugestão de funcionalidade.', 'e1000000-0000-0000-0000-000000000003', 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours'),
    ('3c000000-0000-0000-0000-000000000022', '3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'O relatório de vendas mensal é uma ótima ideia. Vou encaminhar para produto.', 'e1000000-0000-0000-0000-000000000003', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour')
ON CONFLICT (id) DO NOTHING;

-- Ticket 5: João - Acesso
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('5c000000-0000-0000-0000-000000000031', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá! Vi que está com problema de acesso.', 'e1000000-0000-0000-0000-000000000005', 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes'),
    ('5c000000-0000-0000-0000-000000000032', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vou verificar. Pode me informar seu email?', 'e1000000-0000-0000-0000-000000000005', 'text', FALSE, FALSE, NOW() - INTERVAL '25 minutes'),
    ('5c000000-0000-0000-0000-000000000033', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei! IP bloqueado por excesso de tentativas. Desbloqueado.', 'e1000000-0000-0000-0000-000000000005', 'text', FALSE, FALSE, NOW() - INTERVAL '10 minutes')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Contact Tags (junção contacts <-> tags)
-- ============================================================
INSERT INTO contact_tags (contact_id, tag_id) VALUES
    ('d1000000-0000-0000-0000-000000000001', 'c1000000-0000-0000-0000-000000000005'),
    ('d1000000-0000-0000-0000-000000000002', 'c1000000-0000-0000-0000-000000000003'),
    ('d1000000-0000-0000-0000-000000000003', 'c1000000-0000-0000-0000-000000000001'),
    ('d1000000-0000-0000-0000-000000000003', 'c1000000-0000-0000-0000-000000000002'),
    ('d1000000-0000-0000-0000-000000000004', 'c1000000-0000-0000-0000-000000000003'),
    ('d1000000-0000-0000-0000-000000000005', 'c1000000-0000-0000-0000-000000000005')
ON CONFLICT (contact_id, tag_id) DO NOTHING;
