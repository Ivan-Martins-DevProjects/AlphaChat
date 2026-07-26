-- ============================================================
-- AlphaChat - Seed Data para ambiente QAS
-- Tabelas: users, tickets, messages, ticket_participants
-- Nota: migrations 001/002 rodam antes (convertem enum para VARCHAR,
--       renomeiam message->content, criam conversations/contacts/tags)
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
-- Tickets (tela de home/conversas)
-- ============================================================
INSERT INTO tickets (id, customer_id, subject, status, assigned_to, created_at, closed_at) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Plano Enterprise - Proposta', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NULL),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Erro na integração de pagamento', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NULL),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Sugestão: Relatório de vendas', 'open', NULL, NOW() - INTERVAL '3 days', NULL),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'Dúvida sobre planos', 'closed', 'e5f6a7b8-c9d0-1234-efab-345678901234', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Problema de acesso - Telegram', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '2 days', NULL)
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
-- Mensagens
-- ============================================================

-- Ticket 1: Maria - Plano Enterprise
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('1c000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat. Como posso ajudar?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('1c000000-0000-0000-0000-000000000002', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise. Posso te explicar as vantagens?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
    ('1c000000-0000-0000-0000-000000000003', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O plano Enterprise inclui: suporte 24/7, SLA de 1 hora, integrações ilimitadas.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 50 minutes'),
    ('1c000000-0000-0000-0000-000000000004', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O valor é R$ 499,90/mês. Posso enviar a proposta?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 40 minutes'),
    ('1c000000-0000-0000-0000-000000000005', '1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Perfeito! Vou enviar a proposta agora.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '5 minutes')
ON CONFLICT (id) DO NOTHING;

-- Ticket 2: João - Erro Pagamento
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('2c000000-0000-0000-0000-000000000011', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá! Vi que está com problema na integração de pagamento.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('2c000000-0000-0000-0000-000000000012', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Pode me enviar o ID da transação que está falhando?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '23 hours'),
    ('2c000000-0000-0000-0000-000000000013', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei o problema! Timeout na requisição. Vou ajustar para 30s.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '20 hours'),
    ('2c000000-0000-0000-0000-000000000014', '2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Ajuste realizado. Pode testar novamente?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours')
ON CONFLICT (id) DO NOTHING;

-- Ticket 3: Ana - Sugestão
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('3c000000-0000-0000-0000-000000000021', '3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Olá! Recebi sua sugestão de funcionalidade.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours'),
    ('3c000000-0000-0000-0000-000000000022', '3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'O relatório de vendas mensal é uma ótima ideia. Vou encaminhar para produto.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour')
ON CONFLICT (id) DO NOTHING;

-- Ticket 5: João - Acesso
INSERT INTO messages (id, ticket_id, sender_id, content, conversation_id, message_type, is_edited, is_deleted, created_at) VALUES
    ('5c000000-0000-0000-0000-000000000031', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá! Vi que está com problema de acesso.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes'),
    ('5c000000-0000-0000-0000-000000000032', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vou verificar. Pode me informar seu email?', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '25 minutes'),
    ('5c000000-0000-0000-0000-000000000033', '5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei! IP bloqueado por excesso de tentativas. Desbloqueado.', NULL, 'text', FALSE, FALSE, NOW() - INTERVAL '10 minutes')
ON CONFLICT (id) DO NOTHING;
