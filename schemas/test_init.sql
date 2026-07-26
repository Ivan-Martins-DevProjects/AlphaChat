-- Test-only schema initialization: dump + migrations in correct order
-- No seed data - integration tests will create their own test data

\i /docker-entrypoint-initdb.d/00_dump.sql
\i /docker-entrypoint-initdb.d/01_migrations.sql
\i /docker-entrypoint-initdb.d/02_tables.sql
