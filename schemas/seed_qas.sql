-- ============================================================
-- AlphaChat - Seed Data para ambiente QAS
-- Dados de teste para todas as telas
-- ============================================================

-- ============================================================
-- Usuários (tela de login e listagem de atendentes)
-- ============================================================
INSERT INTO users (id, name, email, role, password, profile_pic, created_at) VALUES
    ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Maria Santos', 'maria@alphachat.com', 'admin', '123456', NULL, NOW() - INTERVAL '30 days'),
    ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'João Silva', 'joao@alphachat.com', 'agent', '123456', NULL, NOW() - INTERVAL '25 days'),
    ('d4e5f6a7-b8c9-0123-defa-234567890123', 'Ana Oliveira', 'ana@alphachat.com', 'agent', '123456', NULL, NOW() - INTERVAL '20 days'),
    ('e5f6a7b8-c9d0-1234-efab-345678901234', 'Pedro Costa', 'pedro@alphachat.com', 'agent', '123456', NULL, NOW() - INTERVAL '15 days'),
    ('f6a7b8c9-d0e1-2345-fabc-456789012345', 'Lucia Ferreira', 'lucia@alphachat.com', 'viewer', '123456', NULL, NOW() - INTERVAL '10 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Permissões (já criadas pela migration 001)
-- ============================================================

-- ============================================================
-- Tags (tela de filtros e contato)
-- ============================================================
INSERT INTO tags (id, name, color_code, created_at) VALUES
    ('t1000000-0000-0000-0000-000000000001', 'Urgente', '#ef4444', NOW() - INTERVAL '30 days'),
    ('t1000000-0000-0000-0000-000000000002', 'VIP', '#f59e0b', NOW() - INTERVAL '28 days'),
    ('t1000000-0000-0000-0000-000000000003', 'Enterprise', '#8b5cf6', NOW() - INTERVAL '25 days'),
    ('t1000000-0000-0000-0000-000000000004', 'Suporte', '#10b981', NOW() - INTERVAL '20 days'),
    ('t1000000-0000-0000-0000-000000000005', 'Vendas', '#3b82f6', NOW() - INTERVAL '15 days'),
    ('t1000000-0000-0000-0000-000000000006', 'Novo Cliente', '#06b6d4', NOW() - INTERVAL '10 days'),
    ('t1000000-0000-0000-0000-000000000007', 'Renovação', '#ec4899', NOW() - INTERVAL '5 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Contatos (tela de lista de contatos)
-- ============================================================
INSERT INTO contacts (id, name, phone, email, company, document, notes, profile_pic, tags, owner_id, created_at, updated_at) VALUES
    ('a1000000-0000-0000-0000-000000000001', 'Fernanda Silva', '(11) 99876-5432', 'fernanda@startup.io', 'StartupTech', '123.456.789-00', 'Cliente Enterprise, muito ativa', NULL, '["t1000000-0000-0000-0000-000000000002","t1000000-0000-0000-0000-000000000003"]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NOW()),
    ('a1000000-0000-0000-0000-000000000002', 'Juliana Costa', '(21) 98765-4321', 'juliana@empresa.com.br', 'Costa & Associados', '987.654.321-00', 'Problema com pagamento', NULL, '["t1000000-0000-0000-0000-000000000001"]'::jsonb, 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NOW()),
    ('a1000000-0000-0000-0000-000000000003', 'Carlos Oliveira', '(31) 97654-3210', 'carlos@techcorp.com', 'TechCorp', NULL, 'Sugestão de funcionalidade', NULL, '["t1000000-0000-0000-0000-000000000004"]'::jsonb, NULL, NOW() - INTERVAL '3 days', NOW()),
    ('a1000000-0000-0000-0000-000000000004', 'Roberto Mendes', '(41) 96543-2109', 'roberto@mendes.com', 'Mendes Ltda', NULL, 'Problema de acesso', NULL, '["t1000000-0000-0000-0000-000000000004"]'::jsonb, NULL, NOW() - INTERVAL '2 days', NOW()),
    ('a1000000-0000-0000-0000-000000000005', 'Ana Pereira', '(51) 95432-1098', 'ana@startup.io', 'StartupTech', NULL, 'Interessada no plano Pro', NULL, '["t1000000-0000-0000-0000-000000000005","t1000000-0000-0000-0000-000000000006"]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '1 day', NOW()),
    ('a1000000-0000-0000-0000-000000000006', 'Pedro Santos', '(61) 94321-0987', 'pedro@enterprise.com', 'Enterprise Corp', NULL, 'Novo cliente potencial', NULL, '["t1000000-0000-0000-0000-000000000006"]'::jsonb, NULL, NOW(), NOW()),
    ('a1000000-0000-0000-0000-000000000007', 'Mariana Almeida', '(11) 91234-5678', 'mariana@tech.br', 'TechBR', '456.789.123-00', 'Cliente recorrente', NULL, '["t1000000-0000-0000-0000-000000000007"]'::jsonb, 'd4e5f6a7-b8c9-0123-defa-234567890123', NOW() - INTERVAL '4 days', NOW()),
    ('a1000000-0000-0000-0000-000000000008', 'Lucas Martins', '(21) 98765-1234', 'lucas@digital.com', 'Digital Solutions', NULL, 'Interessado em API', NULL, '["t1000000-0000-0000-0000-000000000005"]'::jsonb, NULL, NOW() - INTERVAL '6 days', NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Tickets (tela de home/conversas)
-- ============================================================
INSERT INTO tickets (id, customer_id, subject, status, assigned_to, contact_id, created_at, closed_at) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Plano Enterprise - Proposta', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'a1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '7 days', NULL),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Erro na integração de pagamento', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1000000-0000-0000-0000-000000000002', NOW() - INTERVAL '5 days', NULL),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Sugestão: Relatório de vendas', 'open', NULL, 'a1000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 days', NULL),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'Dúvida sobre planos', 'closed', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'a1000000-0000-0000-0000-000000000005', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Problema de acesso - Telegram', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1000000-0000-0000-0000-000000000004', NOW() - INTERVAL '2 days', NULL),
    ('6a000000-0000-0000-0000-000000000006', 'f6a7b8c9-d0e1-2345-fabc-456789012345', 'Consulta sobre API', 'waiting', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'a1000000-0000-0000-0000-000000000008', NOW() - INTERVAL '1 day', NULL),
    ('7a000000-0000-0000-0000-000000000007', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Renovação de contrato', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'a1000000-0000-0000-0000-000000000007', NOW() - INTERVAL '4 days', NULL)
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Conversas (tela de chat)
-- ============================================================
INSERT INTO conversations (id, ticket_id, type, owner, read, created_at, updated_at) VALUES
    ('1b000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', 'WhatsApp', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', TRUE, NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 minutes'),
    ('2b000000-0000-0000-0000-000000000002', '2a000000-0000-0000-0000-000000000002', 'Email', 'c3d4e5f6-a7b8-9012-cdef-123456789012', TRUE, NOW() - INTERVAL '5 days', NOW() - INTERVAL '2 hours'),
    ('3b000000-0000-0000-0000-000000000003', '3a000000-0000-0000-0000-000000000003', 'Chat', NULL, TRUE, NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 hour'),
    ('5b000000-0000-0000-0000-000000000005', '5a000000-0000-0000-0000-000000000005', 'Telegram', 'c3d4e5f6-a7b8-9012-cdef-123456789012', TRUE, NOW() - INTERVAL '2 days', NOW() - INTERVAL '10 minutes'),
    ('6b000000-0000-0000-0000-000000000006', '6a000000-0000-0000-0000-000000000006', 'WhatsApp', 'd4e5f6a7-b8c9-0123-defa-234567890123', FALSE, NOW() - INTERVAL '1 day', NOW() - INTERVAL '30 minutes'),
    ('7b000000-0000-0000-0000-000000000007', '7a000000-0000-0000-0000-000000000007', 'Email', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', TRUE, NOW() - INTERVAL '4 days', NOW() - INTERVAL '3 hours')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Mensagens (tela de chat)
-- ============================================================

-- Conversa 1: Fernanda - Plano Enterprise
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('1c000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat. Como posso ajudar?', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
    ('1c000000-0000-0000-0000-000000000002', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise. Posso te explicar as vantagens?', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
    ('1c000000-0000-0000-0000-000000000003', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O plano Enterprise inclui: suporte 24/7, SLA de 1 hora, integrações ilimitadas e relatórios avançados.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 50 minutes'),
    ('1c000000-0000-0000-0000-000000000004', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'O valor é R$ 499,90/mês. Posso enviar a proposta completa por email?', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 40 minutes'),
    ('1c000000-0000-0000-0000-000000000005', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Perfeito! Vou enviar a proposta para fernanda@startup.io agora mesmo.', 'text', FALSE, FALSE, NOW() - INTERVAL '5 minutes')
ON CONFLICT (id) DO NOTHING;

-- Conversa 2: Juliana - Erro Pagamento
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('2c000000-0000-0000-0000-000000000011', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá Juliana, sou o João da AlphaChat. Vi que está com problema na integração de pagamento.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('2c000000-0000-0000-0000-000000000012', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Pode me enviar o ID da transação que está falhando?', 'text', FALSE, FALSE, NOW() - INTERVAL '23 hours'),
    ('2c000000-0000-0000-0000-000000000013', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei o problema! Está havendo um timeout na requisição. Vou ajustar o timeout para 30s.', 'text', FALSE, FALSE, NOW() - INTERVAL '20 hours'),
    ('2c000000-0000-0000-0000-000000000014', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Ajuste realizado. Pode testar novamente? O problema deve estar resolvido.', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours')
ON CONFLICT (id) DO NOTHING;

-- Conversa 3: Carlos - Sugestão
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('3c000000-0000-0000-0000-000000000021', '3b000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Olá Carlos! Aqui é a Ana da AlphaChat. Recebi sua sugestão de funcionalidade.', 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours'),
    ('3c000000-0000-0000-0000-000000000022', '3b000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'O relatório de vendas mensal é uma ótima ideia. Vou encaminhar para nossa equipe de produto.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour')
ON CONFLICT (id) DO NOTHING;

-- Conversa 5: Roberto - Acesso
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('5c000000-0000-0000-0000-000000000031', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Olá Roberto! Sou o João da AlphaChat. Vi que está com problema de acesso.', 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes'),
    ('5c000000-0000-0000-0000-000000000032', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vou verificar seu acesso. Pode me informar seu email?', 'text', FALSE, FALSE, NOW() - INTERVAL '25 minutes'),
    ('5c000000-0000-0000-0000-000000000033', '5b000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Encontrei! Seu IP foi bloqueado por excesso de tentativas. Desbloquei agora. Tente novamente.', 'text', FALSE, FALSE, NOW() - INTERVAL '10 minutes')
ON CONFLICT (id) DO NOTHING;

-- Conversa 6: Lucas - API
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('6c000000-0000-0000-0000-000000000041', '6b000000-0000-0000-0000-000000000006', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Olá Lucas! Sou a Ana da AlphaChat. Vi que tem dúvida sobre nossa API.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day'),
    ('6c000000-0000-0000-0000-000000000042', '6b000000-0000-0000-0000-000000000006', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Nossa API REST documentada está disponível em docs.alphachat.com/api', 'text', FALSE, FALSE, NOW() - INTERVAL '23 hours'),
    ('6c000000-0000-0000-0000-000000000043', '6b000000-0000-0000-0000-000000000006', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Precisa de alguma integração específica? Posso ajudar com documentação.', 'text', FALSE, FALSE, NOW() - INTERVAL '30 minutes')
ON CONFLICT (id) DO NOTHING;

-- Conversa 7: Mariana - Renovação
INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
    ('7c000000-0000-0000-0000-000000000051', '7b000000-0000-0000-0000-000000000007', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá Mariana! Sua assinatura vence em 15 dias. Vou enviar as opções de renovação.', 'text', FALSE, FALSE, NOW() - INTERVAL '4 days'),
    ('7c000000-0000-0000-0000-000000000052', '7b000000-0000-0000-0000-000000000007', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Temos um desconto de 10% para renovação anual. Interessa?', 'text', FALSE, FALSE, NOW() - INTERVAL '3 hours')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- Participantes dos tickets
-- ============================================================
INSERT INTO ticket_participants (ticket_id, user_id, role) VALUES
    ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'admin'),
    ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'agent'),
    ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'agent'),
    ('4a000000-0000-0000-0000-000000000004', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'agent'),
    ('5a000000-0000-0000-0000-000000000005', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'agent'),
    ('6a000000-0000-0000-0000-000000000006', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'agent'),
    ('7a000000-0000-0000-0000-000000000007', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'admin')
ON CONFLICT (ticket_id, user_id) DO NOTHING;
