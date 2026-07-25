-- Migration: Insert fictional seed data
-- Users send messages, contacts are the customers

-- ============================================================
-- Messages - Conversation 1 (WhatsApp - Plano Enterprise)
-- Maria (agent) talking to Fernanda (contact)
-- ============================================================
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('1c000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat. Como posso ajudar?', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('1c000000-0000-0000-0000-000000000002', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise. Posso te explicar as vantagens?', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
    ('1c000000-0000-0000-0000-000000000003', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O plano Enterprise inclui: suporte 24/7, SLA de 1 hora, integrações ilimitadas e relatórios avançados.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 50 minutes'),
    ('1c000000-0000-0000-0000-000000000004', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O valor é R$ 499,90/mês. Posso enviar a proposta completa por email?', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 40 minutes'),
    ('1c000000-0000-0000-0000-000000000005', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Perfeito! Vou enviar a proposta para fernanda@startup.io agora mesmo.', 'text', FALSE, FALSE, NOW() - INTERVAL '5 minutes')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Messages - Conversation 2 (Email - Erro Pagamento)
-- João (agent) talking to Juliana (contact)
-- ============================================================
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('2c000000-0000-0000-0000-000000000011', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá Juliana, sou o João da AlphaChat. Vi que está com problema na integração de pagamento.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('2c000000-0000-0000-0000-000000000012', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Pode me enviar o ID da transação que está falhando?', 'text', FALSE, FALSE, NOW() - INTERVAL '23 hours'),
    ('2c000000-0000-0000-0000-000000000013', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei o problema! Está havendo um timeout na requisição. Vou ajustar o timeout para 30s.', 'text', FALSE, FALSE, NOW() - INTERVAL '20 hours'),
    ('2c000000-0000-0000-0000-000000000014', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Ajuste realizado. Pode testar novamente? O problema deve estar resolvido.', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Messages - Conversation 3 (Chat - Nova Funcionalidade)
-- Ana (agent) talking to Carlos (contact)
-- ============================================================
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('3c000000-0000-0000-0000-000000000021', '3b000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Olá Carlos! Aqui é a Ana da AlphaChat. Recebi sua sugestão de funcionalidade.', 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours'),
    ('3c000000-0000-0000-0000-000000000022', '3b000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'O relatório de vendas mensal é uma ótima ideia. Vou encaminhar para nossa equipe de produto.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Messages - Conversation 5 (Telegram - Acesso)
-- João (agent) talking to Roberto (contact)
-- ============================================================
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('5c000000-0000-0000-0000-000000000031', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá Roberto! Sou o João da AlphaChat. Vi que está com problema de acesso.', 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes'),
    ('5c000000-0000-0000-0000-000000000032', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vou verificar seu acesso. Pode me informar seu email?', 'text', FALSE, FALSE, NOW() - INTERVAL '25 minutes'),
    ('5c000000-0000-0000-0000-000000000033', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei! Seu IP foi bloqueado por excesso de tentativas. Desbloquei agora. Tente novamente.', 'text', FALSE, FALSE, NOW() - INTERVAL '10 minutes')
ON CONFLICT (id) DO NOTHING;
