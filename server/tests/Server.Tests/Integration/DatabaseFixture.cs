using Npgsql;

namespace Server.Tests.Integration;

public class DatabaseFixture : IDisposable
{
    public NpgsqlDataSource DataSource { get; }
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        _connectionString = "Host=localhost;Port=5433;Database=alpha_chat_test;Username=postgres;Password=postgres";
        DataSource = NpgsqlDataSource.Create(_connectionString);
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"
            DELETE FROM messages;
            DELETE FROM contact_tags;
            DELETE FROM conversations;
            DELETE FROM ticket_participants;
            DELETE FROM ticket_events;
            DELETE FROM tickets;
            DELETE FROM contacts;
            DELETE FROM tags;
            DELETE FROM permissions;
            DELETE FROM roles;
            DELETE FROM users;
        ", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SeedTestDataAsync()
    {
        await using var conn = await DataSource.OpenConnectionAsync();

        // Users
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO users (id, name, email, role, password, profile_pic, created_at) VALUES
                ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Maria Santos', 'maria@alphachat.com', 'admin', '123456', NULL, NOW() - INTERVAL '30 days'),
                ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'João Silva', 'joao@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '25 days'),
                ('d4e5f6a7-b8c9-0123-defa-234567890123', 'Ana Oliveira', 'ana@alphachat.com', 'user', '123456', NULL, NOW() - INTERVAL '20 days')
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Roles
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO roles (name, description) VALUES
                ('admin', 'Administrador do sistema com acesso total'),
                ('agent', 'Atendente com acesso a conversas e tickets')
            ON CONFLICT (name) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Permissions
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO permissions (role,
                users_read, users_create, users_update, users_delete,
                conversations_read, conversations_create, conversations_update, conversations_delete,
                messages_read, messages_create, messages_update, messages_delete,
                contacts_read, contacts_create, contacts_update, contacts_delete,
                tickets_read, tickets_create, tickets_update, tickets_delete,
                tags_read, tags_create, tags_update, tags_delete
            ) VALUES
                ('admin', TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE),
                ('agent', TRUE, FALSE, FALSE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE)
            ON CONFLICT (role) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Tags
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO tags (id, name, color_code) VALUES
                ('f1000000-0000-0000-0000-000000000001', 'Enterprise', '#ef4444'),
                ('f1000000-0000-0000-0000-000000000002', 'Urgente', '#f97316'),
                ('f1000000-0000-0000-0000-000000000003', 'Suporte', '#3b82f6')
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Contacts
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO contacts (id, name, phone, email, company, document, notes, profile_pic, tags, owner_id, created_at, updated_at) VALUES
                ('a1000000-0000-0000-0000-000000000001', 'Fernanda Silva', '(11) 99876-5432', 'fernanda@startup.io', 'StartupTech', '123.456.789-00', 'Cliente Enterprise', NULL, '[]'::jsonb, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', NOW() - INTERVAL '7 days', NOW()),
                ('a1000000-0000-0000-0000-000000000002', 'Juliana Costa', '(21) 98765-4321', 'juliana@empresa.com.br', 'Costa & Associados', '987.654.321-00', 'Problema com pagamento', NULL, '[]'::jsonb, 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW() - INTERVAL '5 days', NOW()),
                ('a1000000-0000-0000-0000-000000000003', 'Carlos Oliveira', '(31) 97654-3210', 'carlos@techcorp.com', 'TechCorp', NULL, 'Sugestão de funcionalidade', NULL, '[]'::jsonb, NULL, NOW() - INTERVAL '3 days', NOW())
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Tickets
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO tickets (id, customer_id, subject, status, assigned_to, contact_id, created_at, closed_at) VALUES
                ('1a000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Plano Enterprise - Proposta', 'open', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'a1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '7 days', NULL),
                ('2a000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Erro na integração de pagamento', 'open', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1000000-0000-0000-0000-000000000002', NOW() - INTERVAL '5 days', NULL),
                ('3a000000-0000-0000-0000-000000000003', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Sugestão: Relatório de vendas', 'open', NULL, 'a1000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 days', NULL)
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Conversations
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO conversations (id, type, owner, ticket_id, read, created_at) VALUES
                ('1b000000-0000-0000-0000-000000000001', 'direct', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', '1a000000-0000-0000-0000-000000000001', TRUE, NOW() - INTERVAL '7 days'),
                ('2b000000-0000-0000-0000-000000000002', 'direct', 'c3d4e5f6-a7b8-9012-cdef-123456789012', '2a000000-0000-0000-0000-000000000002', TRUE, NOW() - INTERVAL '5 days'),
                ('3b000000-0000-0000-0000-000000000003', 'direct', 'd4e5f6a7-b8c9-0123-defa-234567890123', '3a000000-0000-0000-0000-000000000003', TRUE, NOW() - INTERVAL '3 days')
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Messages
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO messages (id, ticket_id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at) VALUES
                ('1c000000-0000-0000-0000-000000000001', '1a000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Olá! Aqui é a Maria da AlphaChat.', 'text', FALSE, FALSE, NOW() - INTERVAL '2 hours'),
                ('1c000000-0000-0000-0000-000000000002', '1a000000-0000-0000-0000-000000000001', '1b000000-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Vi que você tem interesse no plano Enterprise.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 hour 55 minutes'),
                ('2c000000-0000-0000-0000-000000000011', '2a000000-0000-0000-0000-000000000002', '2b000000-0000-0000-0000-000000000002', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Vi que está com problema na integração.', 'text', FALSE, FALSE, NOW() - INTERVAL '1 day')
            ON CONFLICT (id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Contact tags
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO contact_tags (contact_id, tag_id) VALUES
                ('a1000000-0000-0000-0000-000000000001', 'f1000000-0000-0000-0000-000000000001'),
                ('a1000000-0000-0000-0000-000000000001', 'f1000000-0000-0000-0000-000000000002')
            ON CONFLICT (contact_id, tag_id) DO NOTHING
        ", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public void Dispose()
    {
        DataSource?.Dispose();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
